using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Grain.CMS
{
    public interface IGrainCache
    {
        void RecieveCMSData(string rawData);
    }

    public interface IGrainUnit
    {
        void Init();
    }

    public static class GrainCMSUtils
    {
        public static GrainCMSUnitCache UnitCacheHandler { get; set; }
        public static GrainCMSUnitPropertiesCache UnitPropertiesCacheHandler { get; set; }
        public static GrainCMSCategoriesCache CategoriesCacheHandler { get; set; }
        public static GrainCMSCategoryDownloader CategoryDownloader{ get; set; }
        public static GrainUnitHandler UnitHandler { get; set; }

        public static string TimeStamp { get; set; }

        public static Unit GetUnitById(string id)
        {
            if (string.IsNullOrEmpty(id) || UnitCacheHandler == null) return null;

            FindResources();

            return UnitCacheHandler.Units.FirstOrDefault(u => u.GetElementValueByIndex("id").Equals(id));
        }

        public static Unit GetUnitByName(string name)
        {
            if (string.IsNullOrEmpty(name) || UnitCacheHandler == null) return null;

            FindResources();

            return UnitCacheHandler.Units.FirstOrDefault(u => u.GetElementValueByIndex("name").Equals(name));
        }

        public static List<string> GetUnitPropertyElements(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            FindResources();

            return UnitPropertiesCacheHandler.PropertyFields.FirstOrDefault(p => p.Key.Equals(key)).Values;
        }

        public static PropertyField GetUnitProperty(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            FindResources();

            return UnitPropertiesCacheHandler.PropertyFields.FirstOrDefault(p => p.Key.Equals(key));
        }

        public static List<string> GetUnitPropertyValuesFromUnits(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            FindResources();

            List<string> temp = new List<string>();

            foreach(Unit unit in UnitCacheHandler.Units)
            {
                string value = unit.GetElementValueByIndex(key);

                if (string.IsNullOrEmpty(value)) continue;

                if(!temp.Contains(value))
                {
                    temp.Add(value);
                }
            }

            return temp;
        }

        public static List<CategoryCollectionContent> GetUnitCategoryContentsByID(string id)
        {
            if (string.IsNullOrEmpty(id) || UnitCacheHandler == null) return null;

            FindResources();

            List<CategoryCollectionContent> temp = new List<CategoryCollectionContent>();

            foreach (Category cat in CategoriesCacheHandler.Categories)
            {
                foreach (CategoryCollection col in cat.collections)
                {
                    foreach (CategoryCollectionContent con in col.contents)
                    {
                        if (con.data.plots.Contains(id))
                        {
                            temp.Add(con);
                        }
                    }
                }
            }

            return temp;
        }

        public static List<CategoryCollectionContent> GetUnitCategoryContentsByName(string name)
        {
            if (string.IsNullOrEmpty(name) || UnitCacheHandler == null) return null;

            FindResources();

            List<CategoryCollectionContent> temp = new List<CategoryCollectionContent>();

            if(CategoriesCacheHandler != null)
            {
                foreach (Category cat in CategoriesCacheHandler.Categories)
                {
                    foreach (CategoryCollection col in cat.collections)
                    {
                        foreach (CategoryCollectionContent con in col.contents)
                        {
                            if (con.data.plots.Contains(GetUnitByName(name).ID))
                            {
                                temp.Add(con);
                            }
                        }
                    }
                }
            }

            return temp;
        }

        public static List<Unit> FindUnits(Dictionary<string, string> criteria, List<Unit> units = null)
        {
            FindResources();

            List<Unit> results = new List<Unit>();

            if (units == null)
            {
                results.AddRange(UnitCacheHandler.Units);
            }
            else
            {
                results.AddRange(units);
            }

            foreach (KeyValuePair<string, string> c in criteria)
            {
                results = Search(c.Key, c.Value, results);
            }

            return results;
        }

        private static List<Unit> Search(string key, string val, List<Unit> units)
        {
            List<Unit> results = new List<Unit>();

            foreach (Unit u in units)
            {
                if (u.GetElementValueByIndex(key).Equals(val))
                {
                    results.Add(u);
                }
            }

            return results;
        }

        public static List<Category> GetCategories()
        {
            FindResources();

            return CategoriesCacheHandler.Categories;
        }

        public static Category GetCategory(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            FindResources();

            return CategoriesCacheHandler.Categories.FirstOrDefault(c => c.name.Equals(name));
        }

        public static CategoryCollection GetCategoryCollection(string category, string collection)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(collection)) return null;

            FindResources();

            Category cat = GetCategory(category);

            if (cat != null)
            {
                return cat.collections.FirstOrDefault(c => c.name.Equals(collection));
            }
            else return null;
        }

        public static void SaveCategories()
        {
            FindResources();

            CategoriesCacheHandler.Save();
        }

        public static int GetTotalCollectionCount(bool onlyUpdates)
        {
            FindResources();

            int count = 0;

            foreach(Category cat in GetCategories())
            {
                foreach(CategoryCollection col in cat.collections)
                {
                    count += (onlyUpdates) ? col.contents.Where(c => c.updated > 0).Count() : col.contents.Count;
                }
            }

            return count;
        }

        private static void FindResources()
        {
            if (UnitCacheHandler == null)
            {
                int count = Resources.FindObjectsOfTypeAll(typeof(GrainCMSUnitCache)).Length;

                if(count > 0) UnitCacheHandler = (GrainCMSUnitCache)Resources.FindObjectsOfTypeAll(typeof(GrainCMSUnitCache))[0];
            }

            if (UnitCacheHandler == null)
            {
                int count = Resources.FindObjectsOfTypeAll(typeof(GrainCMSUnitPropertiesCache)).Length;

                if (count > 0) UnitPropertiesCacheHandler = (GrainCMSUnitPropertiesCache)Resources.FindObjectsOfTypeAll(typeof(GrainCMSUnitPropertiesCache))[0];
            }

            if (CategoriesCacheHandler == null)
            {
                int count = Resources.FindObjectsOfTypeAll(typeof(GrainCMSCategoriesCache)).Length;

                if (count > 0) CategoriesCacheHandler = (GrainCMSCategoriesCache)Resources.FindObjectsOfTypeAll(typeof(GrainCMSCategoriesCache))[0];
            }
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static  bool DeleteFile(string path)
        {
            if (FileExists(path))
            {
                File.Delete(path);

                if (FileExists(path))
                {
                    Debug.Log("File [" + path + "] did not delete successfully");

                    return false;
                }

                Debug.Log("Deleted file [" + path + "]");
            }
            else Debug.Log("File [" + path + "] does not exist");

            return true;
        }

        public static bool CreateFile(string directory)
        {
            DirectoryInfo dInfo = new DirectoryInfo(directory);

            if (!dInfo.Exists) dInfo.Create();
            else
            {
                Debug.Log("Directory [" + directory + "] exists");
                return true;
            }

            if (!FileExists(directory))
            {
                Debug.Log("Directory [" + directory + "] was not created successfully");
                return false;
            }
            else Debug.Log("Directory [" + directory + "] created");

            return true;
        }

        public static bool SaveFile(string directory, string path, byte[] data)
        {
            if (!CreateFile(directory))
            {
                Debug.Log("File [" + path + "] could not be saved. Directory does not exist");
                return false;
            }

            if (!FileExists(path))
            {
                File.WriteAllBytes(path, data);
            }
            else
            {
                if (DeleteFile(path))
                {
                    File.WriteAllBytes(path, data);
                }
            }

            if (!FileExists(path))
            {
                Debug.Log("File [" + path + "] did not save successfully");
                return false;
            }
            else Debug.Log("File [" + path + "] saved");

            return true;
        }

        public static bool ClearFiles(string route)
        {
            bool failed = false;

            string[] directories = Directory.GetFiles(route);

            if (directories.Length > 0)
            {
                foreach (string directory in directories)
                {
                    string[] files = Directory.GetFiles(directory);

                    if (files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (!DeleteFile(files[i])) failed = true;
                        }
                    }

                    Directory.Delete(directory);

                    if (Directory.Exists(directory))
                    {
                        Debug.Log("Failed to delete directory [" + directory + "]");
                        failed = true;
                    }
                }
            }

            if (Directory.GetFiles(route).Length > 0)
            {
                Debug.Log("Failed to delete all directories");
                failed = true;
            }

            return !failed;
        }

        public static List<string> GetDirectorirs(string route)
        {
            List<string> temp = new List<string>();

            string[] directories = Directory.GetFiles(route);

            temp.AddRange(directories.ToList<string>());

            return temp;
        }
    }

}

