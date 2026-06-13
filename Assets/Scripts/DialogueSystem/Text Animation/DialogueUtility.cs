using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class DialogueUtility : MonoBehaviour
{
    // grab the remainder of the text until ">" or end of string
    private const string REMAINDER_REGEX = "(.*?((?=>)|(/|$)))";
    private const string PAUSE_REGEX_STRING = "<p:(?<pause>" + REMAINDER_REGEX + ")>";
    private static readonly Regex PauseRegex = new Regex(PAUSE_REGEX_STRING);
    private const string SPEED_REGEX_STRING = "<sp:(?<speed>" + REMAINDER_REGEX + ")>";
    private static readonly Regex SpeedRegex = new Regex(SPEED_REGEX_STRING);
    private const string ANIM_START_REGEX_STRING = "<anim:(?<anim>" + REMAINDER_REGEX + ")>";
    private static readonly Regex AnimStartRegex = new Regex(ANIM_START_REGEX_STRING);
    private const string ANIM_END_REGEX_STRING = "</anim>";
    private static readonly Regex AnimEndRegex = new Regex(ANIM_END_REGEX_STRING);

    private static readonly Dictionary<string, float> PauseDictionary = new Dictionary<string, float>{
        { "tiny", .1f },
        { "short", .25f },
        { "normal", 0.666f },
        { "long", 1f },
        { "read", 2f },
    };

    public static List<DialogueCommand> ProcessInputString(string message, out string processedMessage)
    {
        List<DialogueCommand> result = new();
        processedMessage = message;

        processedMessage = HandlePauseTags(processedMessage, result);
        processedMessage = HandleSpeedTags(processedMessage, result);
        processedMessage = HandleAnimStartTags(processedMessage, result);
        processedMessage = HandleAnimEndTags(processedMessage, result);

        return result;
    }

    private static string HandleAnimEndTags(string processedMessage, List<DialogueCommand> result)
    {
        var animEndMatches = AnimEndRegex.Matches(processedMessage);
        foreach (Match match in animEndMatches)
        {
            result.Add(new DialogueCommand
            {
                Position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                Type = DialogueCommandType.AnimEnd,
            });
        }
        processedMessage = Regex.Replace(processedMessage, ANIM_END_REGEX_STRING, "");
        return processedMessage;
    }

    private static string HandleAnimStartTags(string processedMessage, List<DialogueCommand> result)
    {
        var animStartMatches = AnimStartRegex.Matches(processedMessage);
        foreach (Match match in animStartMatches)
        {
            var stringVal = match.Groups["anim"].Value;
            result.Add(new DialogueCommand
            {
                Position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                Type = DialogueCommandType.AnimStart,
                TextAnimValue = GetTextAnimationType(stringVal)
            });
        }
        processedMessage = Regex.Replace(processedMessage, ANIM_START_REGEX_STRING, "");
        return processedMessage;
    }

    private static string HandleSpeedTags(string processedMessage, List<DialogueCommand> result)
    {
        var speedMatches = SpeedRegex.Matches(processedMessage);
        foreach (Match match in speedMatches)
        {
            var stringVal = match.Groups["speed"].Value;
            if (!float.TryParse(stringVal, out var val))
            {
                val = 150f;
            }
            result.Add(new DialogueCommand
            {
                Position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                Type = DialogueCommandType.TextSpeedChange,
                FloatValue = val
            });
        }
        processedMessage = Regex.Replace(processedMessage, SPEED_REGEX_STRING, "");
        return processedMessage;
    }

    private static string HandlePauseTags(string processedMessage, List<DialogueCommand> result)
    {
        var pauseMatches = PauseRegex.Matches(processedMessage);
        foreach (Match match in pauseMatches)
        {
            var val = match.Groups["pause"].Value;
            Debug.Assert(PauseDictionary.ContainsKey(val), "no pause registered for '" + val + "'");
            result.Add(new DialogueCommand
            {
                Position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                Type = DialogueCommandType.Pause,
                FloatValue = PauseDictionary[val]
            });
        }
        processedMessage = Regex.Replace(processedMessage, PAUSE_REGEX_STRING, "");
        return processedMessage;
    }

    private static TextAnimationType GetTextAnimationType(string stringVal)
    {
        TextAnimationType result;
        try
        {
            result = (TextAnimationType)Enum.Parse(typeof(TextAnimationType), stringVal, true);
        }
        catch (ArgumentException)
        {
            CustomLog.LogEditorError("Invalid Text Animation Type: " + stringVal);
            result = TextAnimationType.NONE;
        }
        return result;
    }

    private static int VisibleCharactersUpToIndex(string message, int index)
    {
        var result = 0;
        var insideBrackets = false;
        for (var i = 0; i < index; i++)
        {
            if (message[i] == '<')
            {
                insideBrackets = true;
            }
            else if (message[i] == '>')
            {
                insideBrackets = false;
                result--;
            }
            if (!insideBrackets)
            {
                result++;
            }
            else if (i + 6 < index && message.Substring(i, 6) == "sprite")
            {
                result++;
            }
        }
        return result;
    }
}
public struct DialogueCommand
{
    public int Position;
    public DialogueCommandType Type;
    public float FloatValue;
    public string StringValue;
    public TextAnimationType TextAnimValue;
}

public enum DialogueCommandType
{
    Pause,
    TextSpeedChange,
    AnimStart,
    AnimEnd
}

public enum TextAnimationType
{
    NONE,
    SHAKE,
    WAVE
}