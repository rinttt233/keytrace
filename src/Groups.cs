using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Keytrace
{
    public class Group
    {
        public int Id;
        public string Name;
        public long Created;
        public string Dir;

        public string Meta { get { return "g" + Id; } }
    }

    /// <summary>分组（多用户）管理：每个分组一个独立数据目录。</summary>
    public class GroupManager : IDisposable
    {
        private const string DefaultName = "默认";
        private readonly string root;
        private readonly string indexFile;
        private readonly string activeFile;
        private readonly List<Group> groups = new List<Group>();
        private int activeId;
        private Store store;

        public GroupManager(string dataRoot) : this(dataRoot, 0) { }

        public GroupManager(string dataRoot, int initialGroup)
        {
            root = dataRoot;
            Directory.CreateDirectory(root);
            indexFile = Path.Combine(root, "groups.txt");
            activeFile = Path.Combine(root, "active.txt");
            LoadIndex();
            LoadActive();
            if (initialGroup > 0 && Find(initialGroup) != null) activeId = initialGroup;
            if (Find(activeId) == null) activeId = groups[0].Id;
            OpenActive();
        }

        public string RootDir { get { return root; } }
        public IList<Group> All { get { return groups; } }
        public int Count { get { return groups.Count; } }
        public int ActiveId { get { return activeId; } }
        public Group Active { get { return Find(activeId) ?? groups[0]; } }
        public string ActiveName { get { return Active.Name; } }
        public Store Store { get { return store; } }

        public Group Find(int id)
        {
            for (int i = 0; i < groups.Count; i++)
                if (groups[i].Id == id) return groups[i];
            return null;
        }

        public Group ByName(string name)
        {
            for (int i = 0; i < groups.Count; i++)
                if (string.Equals(groups[i].Name, name, StringComparison.CurrentCultureIgnoreCase))
                    return groups[i];
            return null;
        }

        // ---------------------------------------------------------- 读取

        private void LoadIndex()
        {
            bool legacy = File.Exists(Path.Combine(root, "hourly.bin")) ||
                          File.Exists(Path.Combine(root, "journal.bin"));
            if (File.Exists(indexFile))
            {
                try
                {
                    string[] lines = File.ReadAllLines(indexFile, Encoding.UTF8);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.Length == 0 || line[0] == '#') continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length < 2) continue;
                        int id;
                        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) continue;
                        if (id <= 0 || Find(id) != null) continue;
                        Group g = new Group();
                        g.Id = id;
                        g.Name = SanitizeName(parts[1]);
                        if (g.Name.Length == 0) g.Name = "分组 " + id;
                        long created = 0;
                        if (parts.Length > 2)
                            long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out created);
                        g.Created = created;
                        g.Dir = Path.Combine(root, "g" + id);
                        groups.Add(g);
                    }
                }
                catch { }
            }

            if (groups.Count == 0)
            {
                Group g = new Group();
                g.Id = 1;
                g.Name = DefaultName;
                g.Created = Now();
                g.Dir = Path.Combine(root, "g1");
                Directory.CreateDirectory(g.Dir);
                groups.Add(g);
                if (legacy) MoveLegacy(g.Dir);
                SaveIndex();
            }

            for (int i = 0; i < groups.Count; i++)
                try { Directory.CreateDirectory(groups[i].Dir); } catch { }

            groups.Sort(delegate (Group a, Group b) { return a.Id.CompareTo(b.Id); });
        }

        private void MoveLegacy(string dir)
        {
            string[] files = { "hourly.bin", "journal.bin", "hourly.bin.bad" };
            for (int i = 0; i < files.Length; i++)
            {
                string src = Path.Combine(root, files[i]);
                if (!File.Exists(src)) continue;
                try { File.Move(src, Path.Combine(dir, files[i])); } catch { }
            }
        }

        private void LoadActive()
        {
            activeId = 0;
            try
            {
                if (File.Exists(activeFile))
                    int.TryParse(File.ReadAllText(activeFile, Encoding.UTF8).Trim(),
                        NumberStyles.Integer, CultureInfo.InvariantCulture, out activeId);
            }
            catch { }
        }

        private void OpenActive()
        {
            Group g = Find(activeId) ?? groups[0];
            activeId = g.Id;
            store = new Store(g.Dir);
            SaveActive();
        }

        // ---------------------------------------------------------- 保存

        private void SaveIndex()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Keytrace groups v1");
            for (int i = 0; i < groups.Count; i++)
            {
                Group g = groups[i];
                if (g.Dir == null) g.Dir = Path.Combine(root, "g" + g.Id);
                sb.Append(g.Id.ToString(CultureInfo.InvariantCulture)).Append('\t')
                  .Append(g.Name.Replace('\t', ' ')).Append('\t')
                  .Append(g.Created.ToString(CultureInfo.InvariantCulture)).Append('\n');
            }
            string tmp = indexFile + ".tmp";
            try
            {
                File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
                if (File.Exists(indexFile)) File.Replace(tmp, indexFile, null);
                else File.Move(tmp, indexFile);
            }
            catch
            {
                try { File.Copy(tmp, indexFile, true); File.Delete(tmp); } catch { }
            }
        }

        private void SaveActive()
        {
            try { File.WriteAllText(activeFile, activeId.ToString(CultureInfo.InvariantCulture), new UTF8Encoding(false)); }
            catch { }
        }

        public static string SanitizeName(string name)
        {
            if (name == null) return "";
            name = name.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (name.Length > 20) name = name.Substring(0, 20).Trim();
            return name;
        }

        private static long Now()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        // ---------------------------------------------------------- 修改

        public Group Create(string name)
        {
            name = SanitizeName(name);
            if (name.Length == 0) throw new ArgumentException("分组名称不能为空。");
            if (ByName(name) != null) throw new ArgumentException("已存在同名分组。");
            int id = 1;
            for (int i = 0; i < groups.Count; i++)
                if (groups[i].Id >= id) id = groups[i].Id + 1;
            Group g = new Group();
            g.Id = id;
            g.Name = name;
            g.Created = Now();
            g.Dir = Path.Combine(root, "g" + id);
            Directory.CreateDirectory(g.Dir);
            groups.Add(g);
            groups.Sort(delegate (Group a, Group b) { return a.Id.CompareTo(b.Id); });
            SaveIndex();
            return g;
        }

        public void Rename(int id, string name)
        {
            Group g = Find(id);
            if (g == null) return;
            name = SanitizeName(name);
            if (name.Length == 0) throw new ArgumentException("分组名称不能为空。");
            Group same = ByName(name);
            if (same != null && same.Id != id) throw new ArgumentException("已存在同名分组。");
            g.Name = name;
            SaveIndex();
        }

        public bool Delete(int id, out string error)
        {
            error = null;
            Group g = Find(id);
            if (g == null) { error = "分组不存在。"; return false; }
            if (id == activeId) { error = "不能删除正在使用的分组。"; return false; }
            if (groups.Count <= 1) { error = "至少需要保留一个分组。"; return false; }
            groups.Remove(g);
            SaveIndex();
            try { Directory.Delete(g.Dir, true); }
            catch (Exception ex) { error = "记录已从列表移除，但数据目录删除失败：" + ex.Message; }
            return true;
        }

        /// <summary>打开指定分组的数据（不切换当前分组）。</summary>
        public Store Open(int id)
        {
            Group g = Find(id);
            if (g == null) return null;
            return new Store(g.Dir);
        }


        /// <summary>在切换完成后调用：登记新的当前分组并关闭旧存储。</summary>
        public void Activate(int id, Store next)
        {
            Group g = Find(id);
            if (g == null || next == null) return;
            Store old = store;
            store = next;
            activeId = id;
            SaveActive();
            if (old != null && !ReferenceEquals(old, next))
            {
                try { old.Dispose(); } catch { }
            }
        }

        public long GroupBytes(Group g)
        {
            long n = 0;
            try { n += new FileInfo(Path.Combine(g.Dir, "hourly.bin")).Length; } catch { }
            try { n += new FileInfo(Path.Combine(g.Dir, "journal.bin")).Length; } catch { }
            return n;
        }

        public void Dispose()
        {
            if (store != null)
            {
                try { store.Dispose(); } catch { }
                store = null;
            }
        }
    }
}
