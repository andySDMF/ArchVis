using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(ContentSizeFitter))]
public class ScrollViewDisplay : HorizontalLayoutGroup
{
    [SerializeField]
    private string id = "Default";

    [SerializeField]
    private float globalDuration = 1.0f;

    [SerializeField]
    private bool useConstantIndexOnAppend = true;

    [SerializeField]
    private int  constantIndex = 1;

    [SerializeField]
    private GameObject appender;

    [SerializeField]
    private UnityEvent onMoveBegin = new UnityEvent();

    [SerializeField]
    private UnityEvent onMoveEnd = new UnityEvent();

    [SerializeField]
    private Vector2 itemFullSize;

    [SerializeField]
    private Vector2 itemReducedSize;

    [SerializeField]
    private bool setActiveCurrent = false;

    private Dictionary<int, ViewOrderItem> cacheOrder = new Dictionary<int, ViewOrderItem>();
    private MoveThisDirection moveDirecion = MoveThisDirection._Next;

    public System.Action<GameObject> OnSelectEvent { get; set; }
    public GameObject Appender { get { return appender; } }

    public int CurrentIndex
    {
        get
        {
            return cacheOrder.FirstOrDefault(x => x.Value.item.Equals(transform.GetChild(constantIndex))).Key;
        }
    }

    public Dictionary<int, Transform> Order
    {
        get
        {
            Dictionary<int, Transform> items = new Dictionary<int, Transform>();

            foreach(KeyValuePair<int, ViewOrderItem> vItem in cacheOrder)
            {
                items.Add(vItem.Key, vItem.Value.item);
            }

            return items;
        }
    }

    private bool hasInitialised = false;
    private GameObject Selected;

    private new void Start()
    {
        base.Start();

        if(appender == null)
        {
            appender = new GameObject("ScrollViewAppender_" + id);
            appender.transform.SetParent(transform.parent);
            appender.AddComponent<ScrollViewAppender>();

            appender.GetComponent<ScrollViewAppender>().Set(this);

            appender.SetActive(false);
        }
        else
        {
            if(appender.GetComponentsInChildren<ScrollViewAppender>(true).Length <= 0)
            {
                appender.AddComponent<ScrollViewAppender>();
            }

            appender.GetComponentsInChildren<ScrollViewAppender>(true)[0].Set(this);

            appender.SetActive(false);
        }

        Append();

        hasInitialised = true;
    }

    private new void OnEnable()
    {
        base.OnEnable();

        if(hasInitialised) ResetThis();
    }

    public virtual void Append()
    {
        cacheOrder.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ViewOrderItem item = new ViewOrderItem(transform.GetChild(i));
            cacheOrder.Add(i, item);
        }

        ResetThis();
    }

    public virtual void ResetThis()
    {
        foreach (KeyValuePair<int, ViewOrderItem> item in cacheOrder)
        {
            item.Value.ResetThis();
        }

        int key =  (useConstantIndexOnAppend) ? constantIndex : (cacheOrder.Count % 2 <= 0) ? (cacheOrder.Count / 2) : (cacheOrder.Count / 2) -1;
        Selected = transform.GetChild(key).gameObject;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(itemFullSize.x, itemFullSize.y);
        }

        Selected.GetComponent<RectTransform>().sizeDelta = new Vector2(itemReducedSize.x, itemReducedSize.y);
        
        if(setActiveCurrent)
        {
            Selected.SetActive(false);
        }

        if (OnSelectEvent != null)
        {
            OnSelectEvent.Invoke(Selected);
        }
    }

    public virtual void Next()
    {
        moveDirecion = MoveThisDirection._Next;
        Move(1);
    }

    public virtual void Previous()
    {
        moveDirecion = MoveThisDirection._Previous;
        Move(1);
    }

    public virtual void Select(Transform selected)
    {
        if (selected == null) return;

        int key = selected.GetSiblingIndex();
        int currentKey = Selected.transform.GetSiblingIndex();

        if ((key).Equals(currentKey)) return;
        else
        {
            int margin = 1;

            if (key < currentKey)
            {
                moveDirecion = MoveThisDirection._Previous;

                margin = currentKey - key;
                Move(margin);
            }
            else
            {
                margin = key - currentKey;

                moveDirecion = MoveThisDirection._Next;
                Move(margin);
            }
        }
    }

    protected virtual void Move(int margin = 1)
    {
        StopAllCoroutines();

        onMoveBegin.Invoke();

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(itemFullSize.x, itemFullSize.y);

            if (setActiveCurrent)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }

        }

        for (int i = 0; i < transform.childCount; i++)
        {
            int n = 0;

            ViewOrderItem item = cacheOrder.FirstOrDefault(x => x.Value.item.Equals(transform.GetChild(i))).Value;
            item.SetSiblingIndex = MoveThisTo._Nothing;

            if (moveDirecion.Equals(MoveThisDirection._Previous))
            {
                n = i + margin;

                if (n > transform.childCount - 1)
                {
                    n = 0;
                    item.SetSiblingIndex = MoveThisTo._SetsiblingFirst;
                }

                item.Targets = new VectorTarget[1] { new VectorTarget(transform.GetChild(n).position)};
            }
            else
            {
                if(margin > 1)
                {
                    VectorTarget[] targets = new VectorTarget[margin];
                    int temp = i;

                    for(int j = 0; j < margin; j++)
                    {
                        if(temp.Equals(0))
                        {
                            targets[j] = new VectorTarget(transform.GetChild(transform.childCount - 1).position, MoveThisTo._SetSiblingLast);
                            temp = transform.childCount - 1;
                        }
                        else
                        {
                            targets[j] = new VectorTarget(transform.GetChild(temp - 1).position);
                            temp = temp - 1;
                        }
                    }

                    item.Targets = targets;
                }
                else
                {
                    n = i - margin;

                    if (n < 0)
                    {
                        n = transform.childCount - 1;
                        item.SetSiblingIndex = MoveThisTo._SetSiblingLast;
                    }

                    item.Targets = new VectorTarget[1] {new VectorTarget(transform.GetChild(n).position)};
                }
            }
        }

        StartCoroutine(Process());
    }

    protected virtual IEnumerator Process()
    {
        foreach (KeyValuePair<int, ViewOrderItem> item in cacheOrder)
        {
            item.Value.MoveThis(globalDuration);
        }

        float time = 0.0f;

        while(time < globalDuration)
        {
            time += Time.deltaTime;

            yield return null;
        }

        if (OnSelectEvent != null)
        {
            int key = (useConstantIndexOnAppend) ? constantIndex : (cacheOrder.Count % 2 <= 0) ? (cacheOrder.Count / 2) : (cacheOrder.Count / 2) - 1;
            Selected = transform.GetChild(key).gameObject;
            Selected.GetComponent<RectTransform>().sizeDelta = new Vector2(itemReducedSize.x, itemReducedSize.y);

            if (setActiveCurrent)
            {
                Selected.SetActive(false);
            }

            OnSelectEvent.Invoke(transform.GetChild(key).gameObject);
        }

        onMoveEnd.Invoke();
    }

    protected int GetActiveCount()
    {
        int n = 0;

        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy) n++;
        }

        return n;
    }

    protected class ViewOrderItem
    {
        public Transform item;
        public Vector3 origin;
        public int cachedIndex;

        public int TransformIndex { get { return item.GetSiblingIndex(); } }
        public VectorTarget[] Targets { get; set; }
        public Vector3 Current { get; private set; }
        public MoveThisTo SetSiblingIndex { set; get; }

        public ViewOrderItem(Transform t)
        {
            item = t;
            SetSiblingIndex = MoveThisTo._Nothing;
            Current = new Vector3(item.position.x, item.position.y, item.position.z);
            Targets = new VectorTarget[1] {new VectorTarget(new Vector3(item.position.x, item.position.y, item.position.z))};

            cachedIndex = TransformIndex;

            if (item != null)
            {
                origin = new Vector3(item.position.x, item.position.y, item.position.z);
            }
            else origin = Vector3.zero;
        }

        public virtual void ResetThis()
        {
            if (item != null)
            {
                item.position = new Vector3(origin.x, origin.y, origin.z);

                SetSiblingIndex = MoveThisTo._Nothing;
                Current = new Vector3(item.position.x, item.position.y, item.position.z);
                Targets = new VectorTarget[1] { new VectorTarget(new Vector3(item.position.x, item.position.y, item.position.z)) };

                item.SetSiblingIndex(cachedIndex);
                item.localScale = Vector3.one;
            }
        }

        public void MoveThis(float duration)
        {
            item.localScale = Vector3.one;

            if(Targets.Length <= 1)
            {
                switch (SetSiblingIndex)
                {
                    case MoveThisTo._SetsiblingFirst:
                        item.SetAsFirstSibling();
                        item.localScale = Vector3.zero;
                        break;
                    case MoveThisTo._SetSiblingLast:
                        item.SetAsLastSibling();
                        item.localScale = Vector3.zero;
                        break;
                    default:
                        break;
                }
            }

            item.GetComponent<MonoBehaviour>().StopAllCoroutines();

            Current = new Vector3(item.position.x, item.position.y, item.position.z);

            item.GetComponent<MonoBehaviour>().StartCoroutine(Process(duration));
        }

        private IEnumerator Process(float duration)
        {
            float runningTime = 0.0f;
            float percentage = 0.0f;

            for(int i = 0; i < Targets.Length; i++)
            {
                if (Targets.Length > 1) Repostion(Targets[i]);

                while (percentage < 1.0f)
                {
                    runningTime += Time.deltaTime;
                    percentage = runningTime / (duration / Targets.Length);

                    item.position = new Vector3(Ease(Current.x, Targets[i].target.x, percentage), Ease(Current.y, Targets[i].target.y, percentage), 0);

                    yield return null;
                }

                if (percentage >= 1.0f)
                {
                    Current = new Vector3(Targets[i].target.x, Targets[i].target.y, Targets[i].target.z);
                    item.localScale = Vector3.one;
                    item.position = new Vector3(Targets[i].target.x, Targets[i].target.y, Targets[i].target.z);

                    percentage = 0.0f;
                    runningTime = 0.0f;
                }
            }
        }

        private void Repostion(VectorTarget t)
        {
            switch (t.moveTo)
            {
                case MoveThisTo._SetsiblingFirst:
                    item.SetAsFirstSibling();
                    item.localScale = Vector3.zero;
                    break;
                case MoveThisTo._SetSiblingLast:
                    item.SetAsLastSibling();
                    item.localScale = Vector3.zero;
                    break;
                default:
                    break;
            }
        }

        private float Ease(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }
    }

    [System.Serializable]
    protected class VectorTarget
    {
        public Vector3 target;
        public MoveThisTo moveTo;

        public VectorTarget(Vector3 v, MoveThisTo t = MoveThisTo._Nothing)
        {
            target = v;
            moveTo = t;
        }
    }

    protected enum MoveThisTo { _SetsiblingFirst, _SetSiblingLast, _Nothing }
    protected enum MoveThisDirection { _Next, _Previous }
}
