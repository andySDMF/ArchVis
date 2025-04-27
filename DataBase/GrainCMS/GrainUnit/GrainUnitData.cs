using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grain.CMS;

public class GrainUnitData : MonoBehaviour, IGrainUnit
{
    [SerializeField]
    private GetMethod getMethod = GetMethod._Name;

    [Header("Unit Data")]
    [SerializeField]
    private Unit data;

    [Header("Category Data")]
    [SerializeField]
    private List<CategoryReference> categoryContent;

    public Unit Data { get { return data; } }
    public string RawData { get { return JsonUtility.ToJson(data); } }

    public List<CategoryReference> CategoryContent { get { return categoryContent; } }
    public string RawCategoryContent { get { return JsonUtility.ToJson(categoryContent); } }

    private bool hasInit = false;

    public void Init()
    {
        hasInit = true;

        data = (getMethod.Equals(GetMethod._Name)) ? GrainCMSUtils.GetUnitByName(gameObject.name) : GrainCMSUtils.GetUnitById(gameObject.name);

        List<CategoryCollectionContent> temp = (getMethod.Equals(GetMethod._Name)) ? GrainCMSUtils.GetUnitCategoryContentsByName(gameObject.name) : GrainCMSUtils.GetUnitCategoryContentsByID(gameObject.name);

        categoryContent.Clear();

        foreach (CategoryCollectionContent con in temp)
        {
            categoryContent.Add(new CategoryReference(con));
        }
    }

    public enum GetMethod { _Name, _Id }

    [System.Serializable]
    public class CategoryReference
    {
        public string id;
        public string internelURL;
        public string reference;

        public CategoryReference(CategoryCollectionContent v)
        {
            id = v.identifier;
            internelURL = v.internalURL;
            reference = v.reference;
        }
    }
}
