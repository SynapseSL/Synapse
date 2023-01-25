using Hints;
using MEC;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Synapse3.SynapseModule.Player;

/// <summary>
/// Do not use Rich Text 
/// </summary>
public class TextHintList
    : ICollection<SynapseTextHint>
{
    #region Properties & Variables
    static Dictionary<int, int> SizeMaxSide = new Dictionary<int, int>()
    {
        { 1, 40 },//Total per ligne is 81 
        { 2, 21 },//Total per ligne is 43
        { 3, 15 },//Total per ligne is 31
    };

    static Dictionary<int, float> SizeMspace = new Dictionary<int, float>()
    {
        { 1, 0.85f },
        { 2, 1.60f },
        { 3, 2.20f }
    };

    public const int MaxLigne = 54;
    public const int GosthLigne = 15;

    readonly SynapsePlayer _player;
    readonly List<SynapseTextHint> _textHints = new List<SynapseTextHint>();
    
    public ReadOnlyCollection<SynapseTextHint> TextHints => _textHints.AsReadOnly();
    public int Count => TextHints.Count;
    public bool IsReadOnly => false;

    private CoroutineHandle updateCallBack;
    #endregion

    #region Constructor & Destructor
    public TextHintList(SynapsePlayer player)
    {
        _player = player;
    }

    #endregion

    #region Methods
    public void UpdateText()
    {
        Timing.KillCoroutines(updateCallBack);
        RemoveExpierd();

        var count = _textHints.Count;
        if (count == 0) return;
        
        var ligne = new Line[MaxLigne];
        for (int i = 0; i < MaxLigne; i++)
            ligne[i] = new Line();

        foreach (var hint in _textHints.OrderBy(p => p.Priority))
            ProcessHint(ligne, hint);

        var displayTime = _textHints.OrderBy(p => p.DisplayTimeLeft).First(p => p.Displaying).DisplayTimeLeft + 0.01f;
        var playerHint = new Hints.TextHint(GetMessage(ligne), new HintParameter[]
        {
            new StringHintParameter("")
        }, null, displayTime);

        if (count == 1)
        { 
            playerHint._effects = HintEffectPresets.FadeInAndOut(displayTime);
        }

        _player.Hub.hints.Show(playerHint);
        updateCallBack = Timing.RunCoroutine(CallBackUpdateText(displayTime));
    }

    private void RemoveExpierd()
    {
        var length = _textHints.Count;

        for (int i = length - 1; i >= 0; i--)
        {
            var hint = _textHints[i];
            if (!hint.Expired) continue;
            _textHints.RemoveAt(i);
            hint.EndDisplay();
        }
    }


    private void ProcessHint(Line[] lignes, SynapseTextHint hint)
    {
        if (!hint.Displaying) hint.StartDisplay();

        switch (hint.Side)
        {
            case HintSide.Left:
                ProcesseLeft(lignes, hint);
                break;
            case HintSide.Right:
                ProcesseRight(lignes, hint);
                break;
            case HintSide.Midle:
                ProcesseMidle(lignes, hint);
                //TODO:
                break;
        }
    }

    private void ProcesseMidle(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;
        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;
        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].MidleFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Left != null 
                    || lignes[dest - j].Right != null 
                    || lignes[dest - j].Midle != null)
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMult = hint.Size;
            lignes[dest].Midle = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }
    
    private void ProcesseLeft(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;
        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;
        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].LeftFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Left != null)
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMult = hint.Size; 
            lignes[dest].Left = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }

    private void ProcesseRight(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;
        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].RightFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Right != null) 
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMult = hint.Size;
            lignes[dest].Right = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }


    private string GetMessage(Line[] Lignes)
    {
        //<mspace> allow to get char at same size
        //<size>   is the win space for more txt
        var message = "\n";
        for (int i = 0; i < MaxLigne; i++)
        {
            message += Lignes[i];
        }
        message += new string('\n', GosthLigne);

        return message;
    }

    #region List Methods
    private IEnumerator<float> CallBackUpdateText(float time)
    {
        yield return Timing.WaitForSeconds(Math.Min(time, 2));
        UpdateText();//I can't catch the hint of the client (max ammo, item and ect...) so i override them
        yield break;
    }

    public void AddWithoutUpdate(SynapseTextHint hint)
    {
        _textHints.Add(hint);
    }

    public void Add(SynapseTextHint hint)
    {
        _textHints.Add(hint);
        UpdateText();
    }

    public bool Remove(SynapseTextHint hint)
    {
        if (_textHints.Remove(hint))
        {
            UpdateText();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (_textHints.Any())
        {
            _textHints.Clear();
            _player.Hub.hints.Show(new Hints.TextHint("", new HintParameter[]
            {
                new StringHintParameter("")
            }, null, 0.1f));
        }
    }

    public bool Contains(SynapseTextHint hint)
        => _textHints.Contains(hint);

    public void CopyTo(SynapseTextHint[] array, int arrayIndex)
        => _textHints.CopyTo(array, arrayIndex);

    public IEnumerator<SynapseTextHint> GetEnumerator()
        => _textHints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _textHints.GetEnumerator();

    #endregion

    #endregion

    #region Nesteds
    public class Line
    {
        public AnalysedSide Left { get; set; }
        public AnalysedSide Right { get; set; }
        public AnalysedSide Midle { get; set; }
        public bool LeftFree => Left == null && Midle == null && !Gosht;
        public bool RightFree => Right == null && Midle == null && !Gosht;
        public bool MidleFree => Left == null && Right == null && Midle == null && !Gosht;

        public bool Gosht { get; set; } = false;

        public override string ToString()
        {
            if (Gosht) return "";
            string text = "";
            if (Midle == null)
            {
                text += "<align=\"left\">";
                var leftText = Left;
                var rightText = Right;
                if (leftText != null)
                {
                    var charSpace = SizeMspace[(int)Left.SizeMult];
                    if (!leftText.IgnoreReformatage)
                    {
                        text += $"<mspace={charSpace}em><size={Left.SizeMult * 50}%>" + leftText;
                    }
                    else
                    {
                        text += leftText;
                    }
                }
                if (rightText != null)
                {
                    var charSpace = SizeMspace[(int)rightText.SizeMult];
                    var space = new string(' ', SizeMaxSide[(int)rightText.SizeMult] - rightText.TextWithoutTag.Length);

                    if (!rightText.IgnoreReformatage)
                    {
                        text += $"<pos=50%><mspace=0.65em><size=50%> <mspace={charSpace}em><size={rightText.SizeMult * 50}%>";
                        //                                          ^
                        // this space is to sperate the left and right and get a max of 81 char (for the base unite of 1) per line
                        text += space + rightText;
                    }
                    else
                    {
                        text += $"<mspace=0.65em><size=50%> </mspace></size>" + rightText;
                    }
                }
            }
            else
            {
                var midleText = Midle;
                if (!midleText.IgnoreReformatage)
                {
                    text += "<align=\"center\">";
                    var charSpace = SizeMspace[(int)midleText.SizeMult];
                    text += $"<mspace={charSpace}em><size={midleText.SizeMult * 50}%> " + midleText;
                }
                else
                {
                    text += "<align=\"center\">";
                    text += $" " + midleText;
                }
            }
            text += "<size=50%>\n";
            return text;
        }
    }

    public class AnalysedSide
    { 
        public bool IgnoreReformatage { get; set; }
        public string FullText { get; internal set; }
        public string TextWithoutTag { get; internal set; }
        public List<string> Tags { get; internal set; }
        public List<string> NotClosedTags { get; internal set; }
        public float SizeMult { get; internal set; }
        public int LengthWithoutTag => TextWithoutTag.Length;
        public int LengthWithTag => FullText.Length;
        public float Lenght => TextWithoutTag.Length * SizeMult;

        public AnalysedSide(string word, float charSizeMult, bool ignoreReformatage = false) : this(word, charSizeMult, new List<string>())
        {
            IgnoreReformatage = ignoreReformatage;
        }

        public AnalysedSide(string word, float charSizeMult, List<string> notClosedTags)
        {
            FullText = word;
            SizeMult = charSizeMult;
            TextWithoutTag = TextSpliter.TextWithoutTag(word);
            var matches = Regex.Matches(word, "<.*?>");
            Tags = new List<string>();
            NotClosedTags = new List<string>();
            Tags.AddRange(notClosedTags);
            foreach (Match match in matches)
            {
                Tags.Add(match.Value);
            }
            var closingTags = Tags.Where(p => p.StartsWith("</")).ToList();
            var openingTags = Tags.Where(p => !p.StartsWith("</")).ToList();
            for (int i = openingTags.Count - 1; i >= 0; i--)
            {
                int pos = openingTags[i].IndexOf("=");
                string tag = pos >= 0 ? openingTags[i].Substring(0, pos) + ">" : openingTags[i];
                tag = tag.Replace("<", "</");
                if (closingTags.Contains(tag))
                {
                    closingTags.Remove(tag);
                    openingTags.RemoveAt(i);
                }
            }
            NotClosedTags.AddRange(openingTags);
        }
        public override string ToString()
        {
            return FullText;
        }
    }

    public static class TextSpliter
    {
        private const string LessReplace = @"＜";
        private const string GreaterReplace = @"＞";

        public static string TextWithoutTag(string text) => Regex.Replace(text, "<.*?>", string.Empty);

        private static List<string> GetClosingTags(List<string> notClosed)
        {
            var result = new List<string>();
            foreach (var tag in notClosed)
            {
                int pos = tag.IndexOf("=");
                string tagString = pos >= 0 ? tag.Substring(0, pos) + ">" : tag;
                tagString = tagString.Replace("<", "</");
                result.Add(tagString);
            }
            return result;
        }

        private static void ProcessLongWord(string elem, List<string> list, int lineLength, float charSizeMult)
        {
            // mots trop grand a couper sans compter les tag
            float nb = 0;
            int pos = 0;
            bool tag = false;
            string word = "";
            while (pos < elem.Length)
            {
                if (elem[pos] == '<')
                {
                    tag = true;
                }
                else if (tag && elem[pos] == '>')
                {
                    tag = false;
                }
                else
                {
                    nb += tag ? 0 : charSizeMult;
                }
                if (nb > lineLength)
                {
                    nb = 0;
                    list.Add(word);
                    word = "";
                }
                word += elem[pos];
                pos++;
            }
            if (nb > 0)
            {
                list.Add(word);
            }
        }

        public static List<AnalysedSide> Splite(string text, int lineLength, float charSizeMult = 1)
        {
            text = text.Replace(@"\<", LessReplace);
            text = text.Replace(@"\>", GreaterReplace);

            List<AnalysedSide> result = new List<AnalysedSide>();
            if (Regex.Replace(text, "<.*?>", string.Empty).Length * charSizeMult <= lineLength)
            {
                result.Add(new AnalysedSide(text, charSizeMult));
                return result;
            }

            var lstSplit = text.Split(new Char[] { ' ', ',' }).ToList();
            var lst = new List<string>();
            foreach (var elem in lstSplit)
            {
                if (Regex.Replace(elem, "<.*?>", string.Empty).Length * charSizeMult > lineLength)
                {
                    ProcessLongWord(elem, lst, lineLength, charSizeMult);
                }
                else
                {
                    lst.Add(elem);
                }
            }

            var analysedList = new List<AnalysedSide>();
            AnalysedSide previous = new AnalysedSide("", 1, new List<string>());
            foreach (var elem in lst)
            {
                // but reporter les tag non fermer sur le suivant
                previous = new AnalysedSide(elem, charSizeMult, previous.NotClosedTags);
                analysedList.Add(previous);
            }
            // on recree les chaines 
            var basestring = text;
            float curSize = analysedList[0].Lenght;
            int curChar = analysedList[0].FullText.Length;

            List<string> notClosed = new List<string>();
            var count = analysedList.Count;
            for (int i = 1; i < count; i++)
            {
                var element = analysedList[i];
                if ((curSize + charSizeMult + element.Lenght) > lineLength - 1)
                {
                    int pos = basestring.IndexOf(analysedList[i - 1].FullText, curChar - analysedList[i - 1].FullText.Length);
                    string ligne = basestring.Substring(0, pos + analysedList[i - 1].FullText.Length);
                    basestring = basestring.Substring(ligne.Length);
                    ligne = String.Join("", notClosed) + ligne;
                    notClosed = analysedList[i - 1].NotClosedTags;

                    if (notClosed.Any())
                    {
                        var closingTags = GetClosingTags(notClosed);
                        ligne += String.Join("", closingTags);
                    }
                    result.Add(new AnalysedSide(ligne, charSizeMult));
                    curSize = element.Lenght;
                    curChar = element.FullText.Length;

                    continue;
                }
                curSize += element.Lenght + charSizeMult;
                curChar += element.FullText.Length + 1;
            }
            if (!String.IsNullOrEmpty(basestring))
            {
                var closingTags = GetClosingTags(notClosed);
                result.Add(new AnalysedSide(String.Join("", notClosed) + basestring + String.Join("", closingTags), charSizeMult));
            }
            return result;
        }

    }

    #endregion
}
