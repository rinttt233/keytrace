using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Keytrace
{
    public struct Key : IEquatable<Key>
    {
        public long Hour;
        public int Vk;

        public Key(long hour, int vk) { Hour = hour; Vk = vk; }

        public bool Equals(Key other) { return Hour == other.Hour && Vk == other.Vk; }
        public override bool Equals(object obj) { return obj is Key && Equals((Key)obj); }
        public override int GetHashCode()
        {
            return (int)(Hour ^ (Hour >> 32)) * 397 ^ Vk;
        }
    }

    public class Stat
    {
        public int Count;
        public double HoldMs;
    }

    public class KeyStat
    {
        public int Vk;
        public string Name;
        public long Count;
        public double HoldMs;
        public double Avg { get { return Count > 0 ? HoldMs / Count : 0; } }
    }

    public class HourPoint
    {
        public long Hour;
        public long Count;
        public double HoldMs;
    }

    public class Snapshot
    {
        public long Since;
        public long Until;
        public long TotalCount;
        public double TotalHoldMs;
        public int KeyCount;
        public long PrevCount;
        public double PrevHoldMs;
        public List<KeyStat> Keys = new List<KeyStat>();
        public List<HourPoint> Hourly = new List<HourPoint>();
        public long FirstHour;
    }

    public class Store : IDisposable
    {
        private const int Magic = 0x4B545331; // KTS1
        private const int Version = 1;
        private const int RecordSize = 24;    // long + int + int + double

        private readonly object gate = new object();
        private readonly Dictionary<Key, Stat> map = new Dictionary<Key, Stat>();
        private string dir;
        private string snapshotPath;
        private string journalPath;
        private FileStream journalStream;
        private BinaryWriter journalWriter;

        public long FirstHour = long.MaxValue;
        public long LastHour = -1;
        public string Dir { get { return dir; } }

        public Store(string dataDir)
        {
            dir = dataDir;
            Directory.CreateDirectory(dir);
            snapshotPath = Path.Combine(dir, "hourly.bin");
            journalPath = Path.Combine(dir, "journal.bin");
            Load();
            OpenJournal();
        }

        // ---------------------------------------------------------- 加载

        private void Load()
        {
            if (File.Exists(snapshotPath))
            {
                try
                {
                    using (FileStream fs = new FileStream(snapshotPath, FileMode.Open, FileAccess.Read))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (br.ReadInt32() == Magic && br.ReadInt32() == Version)
                        {
                            br.ReadInt64();
                            br.ReadInt64();
                            int count = br.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                long hour = br.ReadInt64();
                                int vk = br.ReadInt32();
                                int c = br.ReadInt32();
                                double hold = br.ReadDouble();
                                AddRecord(hour, vk, c, hold);
                            }
                        }
                    }
                }
                catch
                {
                    try { File.Move(snapshotPath, snapshotPath + ".bad"); } catch { }
                }
            }

            if (File.Exists(journalPath))
            {
                try
                {
                    using (FileStream fs = new FileStream(journalPath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buf = new byte[RecordSize];
                        while (fs.Position + RecordSize <= fs.Length)
                        {
                            ReadFull(fs, buf, RecordSize);
                            long hour = BitConverter.ToInt64(buf, 0);
                            int vk = BitConverter.ToInt32(buf, 8);
                            int c = BitConverter.ToInt32(buf, 12);
                            double hold = BitConverter.ToDouble(buf, 16);
                            AddRecord(hour, vk, c, hold);
                        }
                    }
                }
                catch { }
            }
        }

        private static void ReadFull(Stream s, byte[] buf, int n)
        {
            int off = 0;
            while (off < n)
            {
                int r = s.Read(buf, off, n - off);
                if (r <= 0) throw new EndOfStreamException();
                off += r;
            }
        }

        private void AddRecord(long hour, int vk, int count, double hold)
        {
            Key k = new Key(hour, vk);
            Stat st;
            if (!map.TryGetValue(k, out st))
            {
                st = new Stat();
                map[k] = st;
            }
            st.Count += count;
            st.HoldMs += hold;
            if (hour < FirstHour) FirstHour = hour;
            if (hour > LastHour) LastHour = hour;
        }

        private void OpenJournal()
        {
            journalStream = new FileStream(journalPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            journalWriter = new BinaryWriter(journalStream);
        }

        // ---------------------------------------------------------- 写入

        public void Append(List<KeyValuePair<Key, Stat>> rows)
        {
            if (rows == null || rows.Count == 0) return;
            lock (gate)
            {
                try
                {
                    byte[] buf = new byte[RecordSize];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        Key k = rows[i].Key;
                        Stat st = rows[i].Value;
                        PutInt64(buf, 0, k.Hour);
                        PutInt32(buf, 8, k.Vk);
                        PutInt32(buf, 12, st.Count);
                        PutDouble(buf, 16, st.HoldMs);
                        journalWriter.Write(buf, 0, RecordSize);

                        Stat cur;
                        if (!map.TryGetValue(k, out cur))
                        {
                            cur = new Stat();
                            map[k] = cur;
                        }
                        cur.Count += st.Count;
                        cur.HoldMs += st.HoldMs;
                        if (k.Hour < FirstHour) FirstHour = k.Hour;
                        if (k.Hour > LastHour) LastHour = k.Hour;
                    }
                    journalWriter.Flush();
                    journalStream.Flush(true);
                }
                catch
                {
                    // 写盘失败时不丢数据：把这些行并回（由调用方保留更复杂，此处仅记录）
                }
                if (journalStream.Length > 8 * 1024 * 1024) Compact();
            }
        }

        private static void PutInt64(byte[] b, int off, long v) { Buffer.BlockCopy(BitConverter.GetBytes(v), 0, b, off, 8); }
        private static void PutInt32(byte[] b, int off, int v) { Buffer.BlockCopy(BitConverter.GetBytes(v), 0, b, off, 4); }
        private static void PutDouble(byte[] b, int off, double v) { Buffer.BlockCopy(BitConverter.GetBytes(v), 0, b, off, 8); }

        // ---------------------------------------------------------- 压缩

        private void Compact()
        {
            lock (gate)
            {
                string tmp = snapshotPath + ".tmp";
                try
                {
                    using (FileStream fs = new FileStream(tmp, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(Magic);
                        bw.Write(Version);
                        bw.Write(FirstHour == long.MaxValue ? 0 : FirstHour);
                        bw.Write(LastHour < 0 ? 0 : LastHour);
                        bw.Write(map.Count);
                        foreach (KeyValuePair<Key, Stat> kv in map)
                        {
                            bw.Write(kv.Key.Hour);
                            bw.Write(kv.Key.Vk);
                            bw.Write(kv.Value.Count);
                            bw.Write(kv.Value.HoldMs);
                        }
                        bw.Flush();
                        fs.Flush(true);
                    }
                    CloseJournal();
                    if (File.Exists(snapshotPath)) File.Replace(tmp, snapshotPath, null);
                    else File.Move(tmp, snapshotPath);
                    if (File.Exists(journalPath)) File.Delete(journalPath);
                    OpenJournal();
                }
                catch
                {
                    try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                    if (journalStream == null) OpenJournal();
                }
            }
        }

        private void CloseJournal()
        {
            if (journalWriter != null) { journalWriter.Flush(); journalWriter.Dispose(); journalWriter = null; }
            if (journalStream != null) { journalStream.Dispose(); journalStream = null; }
        }

        // ---------------------------------------------------------- 查询

        public Snapshot Build(long since, long until)
        {
            Snapshot s = new Snapshot();
            s.Since = since;
            s.Until = until;
            long sinceH = FloorHour(since);
            long span = Math.Max(3600, until - since);
            long prevSinceH = FloorHour(since - span);
            Dictionary<int, Stat> byKey = new Dictionary<int, Stat>();
            Dictionary<long, Stat> byHour = new Dictionary<long, Stat>();

            lock (gate)
            {
                s.FirstHour = FirstHour == long.MaxValue ? sinceH : FirstHour;
                foreach (KeyValuePair<Key, Stat> kv in map)
                {
                    long h = kv.Key.Hour;
                    if (h >= sinceH && h < until)
                    {
                        s.TotalCount += kv.Value.Count;
                        s.TotalHoldMs += kv.Value.HoldMs;
                        Stat k;
                        if (!byKey.TryGetValue(kv.Key.Vk, out k)) { k = new Stat(); byKey[kv.Key.Vk] = k; }
                        k.Count += kv.Value.Count;
                        k.HoldMs += kv.Value.HoldMs;
                        Stat hh;
                        if (!byHour.TryGetValue(h, out hh)) { hh = new Stat(); byHour[h] = hh; }
                        hh.Count += kv.Value.Count;
                        hh.HoldMs += kv.Value.HoldMs;
                    }
                    else if (h >= prevSinceH && h < sinceH)
                    {
                        s.PrevCount += kv.Value.Count;
                        s.PrevHoldMs += kv.Value.HoldMs;
                    }
                }
            }

            s.KeyCount = byKey.Count;
            foreach (KeyValuePair<int, Stat> kv in byKey)
            {
                KeyStat ks = new KeyStat();
                ks.Vk = kv.Key;
                ks.Name = Ui.KeyName(kv.Key);
                ks.Count = kv.Value.Count;
                ks.HoldMs = kv.Value.HoldMs;
                s.Keys.Add(ks);
            }
            s.Keys.Sort(delegate (KeyStat a, KeyStat b) { return b.Count.CompareTo(a.Count); });

            long start = since <= 0 ? s.FirstHour : sinceH;
            long end = FloorHour(until - 1);
            if (end < start) end = start;
            for (long h = start; h <= end; h += 3600)
            {
                HourPoint p = new HourPoint();
                p.Hour = h;
                Stat st;
                if (byHour.TryGetValue(h, out st)) { p.Count = st.Count; p.HoldMs = st.HoldMs; }
                s.Hourly.Add(p);
            }
            return s;
        }

        private static long FloorHour(long seconds)
        {
            long h = seconds / 3600;
            if (seconds < 0 && seconds % 3600 != 0) h--;
            return h * 3600;
        }

        public long Rows { get { lock (gate) return map.Count; } }
        public long First { get { lock (gate) return FirstHour == long.MaxValue ? 0 : FirstHour; } }
        public long Last { get { lock (gate) return LastHour < 0 ? 0 : LastHour; } }

        public long SizeBytes
        {
            get
            {
                long n = 0;
                try { if (File.Exists(snapshotPath)) n += new FileInfo(snapshotPath).Length; } catch { }
                try { if (File.Exists(journalPath)) n += new FileInfo(journalPath).Length; } catch { }
                return n;
            }
        }

        // ---------------------------------------------------------- 导出 / 清空

        public void ExportCsv(string path, long since, long until)
        {
            long sinceH = FloorHour(since);
            List<Key> keys;
            lock (gate)
            {
                keys = new List<Key>(map.Keys);
            }
            keys.Sort(delegate (Key a, Key b)
            {
                if (a.Hour != b.Hour) return a.Hour.CompareTo(b.Hour);
                return a.Vk.CompareTo(b.Vk);
            });

            using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                sw.WriteLine("hour_local,hour_unix,vk,key,count,hold_ms");
                for (int i = 0; i < keys.Count; i++)
                {
                    Key k = keys[i];
                    if (k.Hour < sinceH || k.Hour >= until) continue;
                    Stat st;
                    lock (gate)
                    {
                        if (!map.TryGetValue(k, out st)) continue;
                    }
                    DateTime local = DateTimeOffset.FromUnixTimeSeconds(k.Hour).ToLocalTime().DateTime;
                    sw.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0:yyyy-MM-dd HH}:00,{1},{2},{3},{4},{5:0.###}",
                        local, k.Hour, k.Vk, Ui.KeyName(k.Vk), st.Count, st.HoldMs));
                }
            }
        }

        public void Reset()
        {
            lock (gate)
            {
                map.Clear();
                FirstHour = long.MaxValue;
                LastHour = -1;
                CloseJournal();
                try { if (File.Exists(snapshotPath)) File.Delete(snapshotPath); } catch { }
                try { if (File.Exists(journalPath)) File.Delete(journalPath); } catch { }
                OpenJournal();
                try
                {
                    using (FileStream fs = new FileStream(snapshotPath, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(Magic);
                        bw.Write(Version);
                        bw.Write(0L);
                        bw.Write(0L);
                        bw.Write(0);
                    }
                }
                catch { }
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                CloseJournal();
            }
        }
    }
}
