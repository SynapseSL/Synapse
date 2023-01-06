using Hints;
using MEC;
using Synapse3.SynapseModule.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public const int CharPerSide = 50;//real total is 101 per ligne
    public const int MaxLigne = 54;
    public const int GosthLigne = 15;
    public const float DefaultCharSize = 0.34f;
    public const float DefaultTextSize = 50;

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
                //TODO:
                break;
        }
    }

    private void ProcesseLeft(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = TextSpliter.Splite(hint.Text, CharPerSide, hint.Size);
        var ligneCount = textToInsert.Count;
        var startLigne = ligneCount - 1 + hint.Ligne * hint.Size;
        if (startLigne >= MaxLigne) return;

        for (int ligneIndex = startLigne; ligneIndex >= 0; ligneIndex--)
        {
            var ligne = lignes[ligneIndex + 1];
            if (!ligne.Gosht && ligne.Left != null ) return; 
        }

        for (int ligneIndex = startLigne; ligneIndex >= 0; ligneIndex--)
        {
            if (ligneIndex % hint.Size == 0)
            {
                lignes[ligneIndex + 1].Left = textToInsert[ligneIndex];
            }
            else
            {
                lignes[ligneIndex + 1].Gosht = true;
            }
        }
    }

    private void ProcesseRight(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = TextSpliter.Splite(hint.Text, CharPerSide, hint.Size);
        var ligneCount = textToInsert.Count;
        var startLigne = ligneCount - 1 + hint.Ligne * hint.Size;
        if (startLigne >= MaxLigne) return;

        for (int ligneIndex = startLigne; ligneIndex >= 0; ligneIndex--)
        {
            var ligne = lignes[ligneIndex + 1];
            if (!ligne.Gosht && ligne.Right != null) return;
        }

        for (int ligneIndex = startLigne; ligneIndex >= 0; ligneIndex--)
        {
            if (ligneIndex % hint.Size == 0)
            {
                lignes[ligneIndex + 1].Right = textToInsert[ligneIndex];
            }
            else
            {
                lignes[ligneIndex + 1].Gosht = true;
            }
        }
    }

    private string GetMessage(Line[] Lignes)
    {
        //<mspace> allow to get char at same size
        //<size>   is the win space for more txt
        var message = "";
        for (int ligne = 0; ligne < MaxLigne; ligne++)
        {
#if DEBUG
            message += "<align=\"left\">" + (ligne % 10);
#else
            message += "<align="left"> "; //the Space is Bc 101 char per ligne and not 100
#endif
            message += ligne;
        }
        message += new string('\n', GosthLigne);

        return message;
    }

    #region List Methods
    private IEnumerator<float> CallBackUpdateText(float time)
    {
        yield return Timing.WaitForSeconds(time);
        UpdateText();
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
        _textHints.Clear();
        UpdateText();
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
        public bool Gosht { get; set; } = false;

        public override string ToString()
        {
            if (Gosht) return "";

            var text = "";
            var leftText = Left;
            var rightText = Right;
            if (leftText != null)
            {
                text += leftText;
            }

            if (rightText != null)
            {
                var space = new string(' ', CharPerSide - Right.TextWithoutTag.Length);
                text += "<pos=50%>" + space + rightText;
            }
            text += "\n";
            return text;
        }
    }



    public class AnalysedSide
    { 
        public string FullText { get; private set; }
        public string TextWithoutTag { get; private set; }
        public List<string> Tags { get; private set; }
        public List<string> NotClosedTags { get; private set; }
        public float SizeMult { get; private set; }
        public int LengthWithoutTag => TextWithoutTag.Length;
        public int LengthWithTag => FullText.Length;
        public float Size => TextWithoutTag.Length * SizeMult;

        public AnalysedSide(string word, float charSizeMult) : this(word, charSizeMult, new List<string>())
        {

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
            float curSize = analysedList[0].Size;
            int curChar = analysedList[0].FullText.Length;

            List<string> notClosed = new List<string>();
            for (int i = 1; i < analysedList.Count; i++)
            {
                var element = analysedList[i];
                if ((curSize + charSizeMult + element.Size) > lineLength - 1)
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
                    curSize = element.Size;
                    curChar = element.FullText.Length;

                    continue;
                }
                curSize += element.Size + charSizeMult;
                curChar += element.FullText.Length + 1;
            }
            if (!String.IsNullOrEmpty(basestring))
            {
                result.Add(new AnalysedSide(String.Join("", notClosed) + basestring, charSizeMult));
            }
            return result;
        }

    }

    #endregion
}
