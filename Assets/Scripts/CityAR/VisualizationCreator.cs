using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace CityAR
{
    public class VisualizationCreator : MonoBehaviour
    {

        public static string mode = "number of lines";
        public GameObject districtPrefab;
        public GameObject buildingPrefab;
        private DataObject _dataObject;
        private GameObject _platform;
        private Data _data;

        private void Start()
        {
            _platform = GameObject.Find("Platform");
            _data = _platform.GetComponent<Data>();
            _dataObject = _data.ParseData();
            BuildCity(_dataObject);
        }
        
        public void BuildCity()
        {
            BuildCity(_dataObject);
        }

        private void BuildCity(DataObject p)
        {
            if (p.project.files.Count > 0)
            {
                p.project.w = 1;
                p.project.h = 1;
                p.project.deepth = 1;
                BuildDistrict(p.project, false);
            }
        }

        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * splitHorizontal: Specifies whether the subsequent children should be split horizontally or vertically along the parent
         */
        private void BuildDistrict(Entry entry, bool splitHorizontal)
        {
            if (entry.type.Equals("File"))
            {
                //TODO if entry is from type File, create building
                entry.color = GetColorForDepth(entry.numberOfMethods / 10);

                BuildBuilding(entry);
            }
            else
            {
                float x = entry.x;
                float z = entry.z;

                float dirLocs = entry.numberOfLines;
                entry.color = GetColorForDepth(entry.deepth);

                BuildDistrictBlock(entry, false);

                foreach (Entry subEntry in entry.files) {
                    subEntry.x = x;
                    subEntry.z = z;
                    
                    if (subEntry.type.Equals("Dir"))
                    {
                        float ratio = subEntry.numberOfLines / dirLocs;
                        subEntry.deepth = entry.deepth + 1;

                        if (splitHorizontal) {
                            subEntry.w = ratio * entry.w; // split along horizontal axis
                            subEntry.h = entry.h;
                            x += subEntry.w;
                        } else {
                            subEntry.w = entry.w;
                            subEntry.h = ratio * entry.h; // split along vertical axis
                            z += subEntry.h;
                        }
                    }
                    else
                    {
                        subEntry.parentEntry = entry;
                    }
                    BuildDistrict(subEntry, !splitHorizontal);
                }

                if (!splitHorizontal)
                {
                    entry.x = x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.h = 1f - z;
                    }
                    entry.deepth += 1;
                    BuildDistrictBlock(entry, true);
                }
                else
                {
                    entry.x = -x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.w = 1f - x;
                    }
                    entry.deepth += 1;
                    BuildDistrictBlock(entry, true);
                }
            }
        }

        private void BuildBuilding(Entry entry)
        {
            Transform myTransform = entry.parentEntry.goc.gameObject.transform;
            GameObject prefabInstance = Instantiate(buildingPrefab, myTransform,true);
            
            prefabInstance.name = entry.name.TrimEnd(".java".ToCharArray());
            EntryData entryData = prefabInstance.AddComponent<EntryData>();
            entryData.entry = entry;
            prefabInstance.transform.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
            
            prefabInstance.transform.localScale = new Vector3(0.05f, calculateHight(entry), 0.05f);
            entry.parentEntry.goc.UpdateCollection();
        }

        private float calculateHight(Entry entry)
        {
            float hight = 0f;
            switch (mode)
            {
                case "number of lines":
                {
                    hight = 10f * (float) Math.Log(entry.numberOfLines);
                    break;
                }
                case "number of methods":
                {
                    hight = entry.numberOfMethods;
                    break;
                    
                }
                case "number of abstract classes":
                {
                    hight = entry.numberOfAbstractClasses * 10;
                    break; 
                }
                case "number of interfaces":
                {
                    hight = entry.numberOfInterfaces * 10;
                    break;
                }
            }
            return hight;
        }
            

        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * isBase: If true, the entry has no further subfolders. Buildings must be placed on top of the entry
         */
        private void BuildDistrictBlock(Entry entry, bool isBase)
        {
            if (entry == null)
            {
                return;
            }
            
            float w = entry.w; // w -> x coordinate
            float h = entry.h; // h -> z coordinate
            
            if (w * h > 0)
            {
                GameObject prefabInstance = Instantiate(districtPrefab, _platform.transform, true);

                if (!isBase)
                {
                    prefabInstance.name = entry.name;
                    prefabInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = entry.color;
                    prefabInstance.transform.localScale = new Vector3(entry.w, 1f,entry.h);
                    prefabInstance.transform.localPosition = new Vector3(entry.x, entry.deepth, entry.z);
                }
                else
                {
                    prefabInstance.SetActive(false);
                    prefabInstance.name = entry.name+"Base";
                    prefabInstance.transform.GetChild(0).rotation = Quaternion.Euler(90,0,0);
                    prefabInstance.transform.localScale = new Vector3(entry.w, 1,entry.h);
                    prefabInstance.transform.localPosition = new Vector3(entry.x, entry.deepth+0.001f, entry.z);
                    
                }

                Vector3 scale = prefabInstance.transform.localScale;
                float scaleX = scale.x - (entry.deepth * 0.005f);
                float scaleZ = scale.z - (entry.deepth * 0.005f);
                float shiftX = (scale.x - scaleX) / 2f;
                float shiftZ = (scale.z - scaleZ) / 2f;
                prefabInstance.transform.localScale = new Vector3(scaleX, scale.y, scaleZ);
                Vector3 position = prefabInstance.transform.localPosition;
                prefabInstance.transform.localPosition = new Vector3(position.x - shiftX, position.y, position.z + shiftZ);
                
                //TODO add GridObject Collection to position buildings
                entry.goc = prefabInstance.AddComponent<GridObjectCollection>();
                entry.goc.Anchor = LayoutAnchor.UpperLeft;
                int childCount = prefabInstance.transform.childCount;
                entry.goc.Rows = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                entry.goc.Columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                entry.goc.Layout = LayoutOrder.ColumnThenRow;
                entry.goc.CellHeight = 0.1f;
                entry.goc.CellWidth = 0.1f;
            }
        }

        private bool ContainsDirs(Entry entry)
        {
            foreach (Entry e in entry.files)
            {
                if (e.type.Equals("Dir"))
                {
                    return true;
                }
            }

            return false;
        }
        
        private Color GetColorForDepth(int depth)
        {
            Color color;
            switch (depth)
            {
                case 1:
                    color = new Color(179f / 255f, 209f / 255f, 255f / 255f);
                    break;
                case 2:
                    color = new Color(128f / 255f, 179f / 255f, 255f / 255f);
                    break;
                case 3:
                    color = new Color(77f / 255f, 148f / 255f, 255f / 255f);
                    break;
                case 4:
                    color = new Color(26f / 255f, 117f / 255f, 255f / 255f);
                    break;
                case 5:
                    color = new Color(0f / 255f, 92f / 255f, 230f / 255f);
                    break;
                default:
                    color = new Color(0f / 255f, 71f / 255f, 179f / 255f);
                    break;
            }

            return color;
        }
        
        public class EntryData : MonoBehaviour
        {
            public Entry entry;

            public EntryData(Entry entry)
            {
                this.entry = entry;
            }
        }
    }
}