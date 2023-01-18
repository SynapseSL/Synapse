using Hints;
using Mirror;
using Synapse3.SynapseModule.Enums;
using System;
using System.Collections.Generic;

namespace Synapse3.SynapseModule.Player;

public class SynapseTextHint
{
    public HintSide Side { get; }
    public int Size { get; }
    public int Priority { get; }

    public bool Displayed { get; internal set; }
    public bool Displaying { get; internal set; }
    public float DisplayTimeLeft => _displayRemoveTime - (float)NetworkTime.time;
    public bool Expired => Displaying && DisplayTimeLeft <= 0;
    public float DisplayTime { get; set; }
    public string Text { get; set; }
    public int Ligne { get; set; }
    public bool IgnoreParsing { get; set; }


    private float _displayRemoveTime;

    /// <summary>
    /// Do not use size, ligne or space balise!
    /// if you whant use "<" do "\<"
    /// </summary>
    public SynapseTextHint(int ligne, string text, float displayTime, HintSide side, int size = 1, int priority = 500)
    {
        DisplayTime = displayTime;
        Size = Math.Max(Math.Min(size, 3), 1);
        Ligne = Math.Max(Math.Min(ligne, TextHintList.MaxLigne - 1), 0);
        Side = side;
        Priority = priority;
        Text = text.Replace("\n", "");

    }

    public void EndDisplay()
    {
        Displayed = true;
        Displaying = false;
    }

    public void StartDisplay()
    {
        _displayRemoveTime = (float)NetworkTime.time + DisplayTime;
        Displaying = true;
    }

    public void ResetDisplay()
    {
        Displayed = false;
        Displaying = false;
    }

}
