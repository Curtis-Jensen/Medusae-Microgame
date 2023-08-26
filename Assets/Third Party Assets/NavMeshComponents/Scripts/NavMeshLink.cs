using System.Collections.Generic;


    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_StartPoint = new Vector3(0.0f, 0.0f, -2.5f);
        public Vector3 startPoint { get { return m_StartPoint; } set { m_StartPoint = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_EndPoint = new Vector3(0.0f, 0.0f, 2.5f);
        public Vector3 endPoint { get { return m_EndPoint; } set { m_EndPoint = value; UpdateLink(); } }

        [SerializeField]
        float m_Width;
        public float width { get { return m_Width; } set { m_Width = value; UpdateLink(); } }

        [SerializeField]
        int m_CostModifier = -1;
        public int costModifier { get { return m_CostModifier; } set { m_CostModifier = value; UpdateLink(); } }

        [SerializeField]
        bool m_Bidirectional = true;
        public bool bidirectional { get { return m_Bidirectional; } set { m_Bidirectional = value; UpdateLink(); } }

        [SerializeField]
        bool m_AutoUpdatePosition;
        public bool autoUpdate { get { return m_AutoUpdatePosition; } set { SetAutoUpdate(value); } }

        [SerializeField]
        int m_Area;
        public int area { get { return m_Area; } set { m_Area = value; UpdateLink(); } }

        NavMeshLinkInstance linkInstance = new NavMeshLinkInstance();

        Vector3 lastPosition = Vector3.zero;
        Quaternion lastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && linkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            linkInstance.Remove();
        }

        public void UpdateLink()
        {
            linkInstance.Remove();
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (m_AutoUpdatePosition == value)
                return;
            m_AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (linkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData();
            link.startPosition = m_StartPoint;
            link.endPosition = m_EndPoint;
            link.width = m_Width;
            link.costModifier = m_CostModifier;
            link.bidirectional = m_Bidirectional;
            link.area = m_Area;
            link.agentTypeID = m_AgentTypeID;
            linkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (linkInstance.valid)
                linkInstance.owner = this;

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (lastPosition != transform.position) return true;
            if (lastRotation != transform.rotation) return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Width = Mathf.Max(0.0f, m_Width);

            if (!linkInstance.valid)
                return;

            UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }

