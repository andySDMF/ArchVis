using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class LineControllerGroup : MonoBehaviour
{
    [SerializeField]
    private UnityEvent createCompleted = new UnityEvent();

    [SerializeField]
    private List<LineController> controllers = new List<LineController>();

    public string Last { get; set; }

    public void Create()
    {
        controllers.ForEach(c => c.Create(false));

        createCompleted.Invoke();
    }

    public void Show(string id = "")
    {
        LineController con = controllers.FirstOrDefault(c => c.ID.Equals(id));

        if(con != null)
        {
            Last = id;

            con.Draw();
        }
    }

    public void Hide(string id = "")
    {
        LineController con = controllers.FirstOrDefault(c => c.ID.Equals(id));

        if (con != null)
        {
            con.Hide();
        }
    }

    public void ShowAndHide(string show, string hide)
    {
        if(show.Equals(hide))
        {
            Show(show);

            return;
        }

        if (string.IsNullOrEmpty(hide) || hide.Equals("All") || hide.Equals("Group"))
        {
            foreach (LineController con in controllers)
            {
                if (con.ID.Equals(show)) continue;

                con.Hide();
            }
        }
        else Hide(hide);

        Show(show);
    }

    public void ShowGroup(string show)
    {
        if (show == null) return;

        string[] cons = show.Split(',');

        foreach (LineController con in controllers)
        {
            if (cons.Contains(con.ID))
            {
                con.Draw();
            }
            else
            {
                con.Hide();
            }
        }

        Last = "Group";
    }

    public void Toggle(bool state)
    {
        if (state)
        {
            Last = "All";
            controllers.ForEach(c => c.Draw());
        }
        else controllers.ForEach(c => c.Hide());
    }

    public void Animate(List<LineAnimation> animations)
    {
        controllers.ForEach(c => c.Hide());

        foreach(LineAnimation lAni in animations)
        {
            LineController lCon = controllers.FirstOrDefault(c => c.ID.Equals(lAni.name));

            if(lCon != null)
            {
                lCon.Animate(lAni.delay);
            }
        }

        Last = "Group";
    }

    public class LineAnimation
    {
        public string name = "";
        public float delay = 0.0f;

        public LineAnimation(string n, float d)
        {
            name = n;
            delay = d;
        }
    }
}
