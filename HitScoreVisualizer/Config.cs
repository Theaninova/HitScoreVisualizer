using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HitScoreVisualizer
{
    class Config
    {
        public static Config instance;

        public const string mode_hardcoreCompetetive = "hardcoreCompetetive";
        public const string mode_format = "format";
        public const string mode_textOnly = "textOnly";
        public const string mode_numeric = "numeric";
        public const string mode_scoreOnTop = "scoreOnTop";

        public const char format_indicator = '%';
        public const char format_beforeCutScore = 'b';
        public const char format_accuracyScore = 'c';
        public const char format_afterCutScore = 'a';
        public const char FORMAT_BEFORECUTSCORE = 'B';
        public const char FORMAT_ACCUTRACYSCORE = 'C';
        public const char FORMAT_AFTERCUTSCORE = 'A';
        public const char format_score = 's';
        public const char format_indicatorChar = format_indicator;
        public const char format_lineBreak = 'n';

        public const bool percentages = true;
        public const int maxBeforeCutScore = 70;
        public const int maxAfterCutScore = 30;
        public const int maxAccuracyScore = 10;

        // If true, this config will not overwrite the existing config file.
        // (This gets set if a config from a newer version is detected.)
        [JsonIgnore]
        public bool noSerialize;

        public struct Judgment
        {
            // This judgment will be applied only to notes hit with score >= this number.
            // Note that if no judgment can be applied to a note, the text will appear as in the unmodded
            // game.
            [DefaultValue(0)]
            public int threshold;

            // The text to display (if judgment text is enabled).
            [DefaultValue("")]
            public string text;

            // 4 floats, 0-1; red, green, blue, glow (not transparency!)
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)] // leaving this out should look obviously wrong
            public float[] color;

            // If true, the text color will be lerped between this judgment's color and the previous
            // based on how close to the next threshold it is.
            // Specifying fade : true for the first judgment in the array is an error, and will crash the
            // plugin.
            [DefaultValue(false)]
            public bool fade;
        }

        // Judgments for individual parts of the swing (angle before, angle after, accuracy).
        public struct SegmentJudgment
        {
            // This judgment will be applied only when the appropriate part of the swing contributes score >= this number.
            // If no judgment can be applied, the judgment for this segment will be "" (the empty string).
            [DefaultValue(0)]
            public int threshold;
            // The text to replace the appropriate judgment specifier with (%B, %C, %A) when this judgment applies.
            public string text;
        }

        // If the version number (excluding patch version) of the config is higher than that of the plugin,
        // the config will not be loaded. If the version number of the config is lower than that of the
        // plugin, the file will be automatically converted. Conversion is not guaranteed to occur, or be
        // accurate, across major versions.
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int majorVersion;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int minorVersion;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int patchVersion;

        // If this is true, the config will be overwritten with the plugin's default settings after an
        // update rather than being converted.
        public bool isDefaultConfig;

        // If set to "format", displays the judgment text, with the following format specifiers allowed:
        // - %b: The score contributed by the part of the swing before cutting the block.
        // - %c: The score contributed by the accuracy of the cut.
        // - %a: The score contributed by the part of the swing after cutting the block.
        // - %B, %C, %A: As above, except using the appropriate judgment from that part of the swing (as configured for "beforeCutAngleJudgments", "accuracyJudgments", or "afterCutAngleJudgments").
        // - %s: The total score for the cut.
        // - %%: A literal percent symbol.
        // - %n: A newline.
        //
        // If set to "numeric", displays only the note score.
        // If set to "textOnly", displays only the judgment text.
        // If set to "scoreOnTop", displays both (numeric score above judgment text).
        // Otherwise, displays both (judgment text above numeric score).
        [DefaultValue("")]
        public string displayMode;

        // Order from highest threshold to lowest; the first matching judgment will be applied
        public Judgment[] judgments;

        // Judgments for the part of the swing before cutting the block (score is from 0-70).
        // Format specifier: %B
        public SegmentJudgment[] beforeCutAngleJudgments;

        // Judgments for the accuracy of the cut (how close to the center of the block the cut was, score is from 0-10).
        // Format specifier: %C
        public SegmentJudgment[] accuracyJudgments;

        // Judgments for the part of the swing after cutting the block (score is from 0-30).
        // Format specifier: %A
        public SegmentJudgment[] afterCutAngleJudgments;

        // path to where the config is saved
        private const string FILE_PATH = "/UserData/HitScoreVisualizerConfig.json";

        private const string DEFAULT_JSON = @"{
  ""majorVersion"": 2,
  ""minorVersion"": 1,
  ""patchVersion"": 0,
  ""displayMode"": ""format"",
  ""judgments"": [
    {
      ""threshold"": 110,
      ""text"": ""Fantastic%n%s%n%B %C %A"",
      ""color"": [
        1.0,
        1.0,
        1.0,
        1.0
      ]
    },
    {
      ""threshold"": 101,
      ""text"": ""<size=80%>Excellent</size>%n%s%n%B %C %A"",
      ""color"": [
        0.0,
        1.0,
        0.0,
        1.0
      ]
},
    {
      ""threshold"": 90,
      ""text"": ""<size=80%>Great</size>%n%s%n%B %C %A"",
      ""color"": [
        1.0,
        0.980392158,
        0.0,
        1.0
      ]
    },
    {
      ""threshold"": 80,
      ""text"": ""<size=80%>Good</size>%n%s%n%B %C %A"",
      ""color"": [
        1.0,
        0.6,
        0.0,
        1.0
      ],
      ""fade"": true
    },
    {
      ""threshold"": 60,
      ""text"": ""<size=80%>Decent</size>%n%s%n%B %C %A"",
      ""color"": [
        1.0,
        0.0,
        0.0,
        1.0
      ],
      ""fade"": true
    },
    {
      ""text"": ""<size=80%>Way Off</size>%n%s%n%B %C %A"",
      ""color"": [
        0.5,
        0.0,
        0.0,
        1.0
      ],
      ""fade"": true
    }
  ],
  ""beforeCutAngleJudgments"": [
    {
	  ""threshold"": 70,
      ""text"": ""+""
    },
    {
	  ""threshold"": 35,
      ""text"": ""  ""
    },
    {
	  ""threshold"": 0,
      ""text"": ""-""
    }
  ],
  ""accuracyJudgments"": [
    {
	  ""threshold"": 10,
      ""text"": ""+""
    },
    {
	  ""threshold"": 5,
      ""text"": ""  ""
    },
    {
	  ""threshold"": 0,
      ""text"": ""-""
    }
  ],
  ""afterCutAngleJudgments"": [
    {
	  ""threshold"": 30,
      ""text"": ""+""
    },
    {
	  ""threshold"": 15,
      ""text"": ""  ""
    },
    {
	  ""threshold"": 0,
      ""text"": ""-""
    }
  ]
}";
        public static readonly Config DEFAULT_CONFIG = JsonConvert.DeserializeObject<Config>(DEFAULT_JSON,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });

        public static readonly Judgment DEFAULT_JUDGMENT = new Judgment
        {
            threshold = 0,
            text = "",
            color = new float[] { 1f, 1f, 1f, 1f },
            fade = false
        };

        public static readonly SegmentJudgment DEFAULT_SEGMENT_JUDGMENT = new SegmentJudgment
        {
            threshold = 0,
            text = ""
        };

        public static string fullPath => Environment.CurrentDirectory.Replace('\\', '/') + FILE_PATH;

        public static void load()
        {
            Plugin.log("Loading config...");
            if (!File.Exists(fullPath))
            {
                Plugin.log("Writing default config.");
                // if the config file doesn't exist, save the default one
                resetToDefault();
                save(true);
                return;
            }
            string configJson = File.ReadAllText(fullPath);
            Config loaded = JsonConvert.DeserializeObject<Config>(configJson,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
            if (!validate(loaded))
            {
                Plugin.log("Falling back to default config (file will not be overwritten)");
                // don't try to modify the original default when disabling serialization
                instance = DEFAULT_CONFIG.MemberwiseClone() as Config;
                // since we couldn't read the existing config, don't overwrite it
                instance.noSerialize = true;
                return;
            }
            if (outdated(loaded))
            {
                if (loaded.isDefaultConfig)
                {
                    loaded = DEFAULT_CONFIG;
                    instance = loaded;
                    save();
                    return;
                }
                // put config update logic here
                if (loaded.majorVersion == 2 && loaded.minorVersion == 0)
                {
                    loaded.beforeCutAngleJudgments = new SegmentJudgment[] { DEFAULT_SEGMENT_JUDGMENT };
                    loaded.accuracyJudgments = new SegmentJudgment[] { DEFAULT_SEGMENT_JUDGMENT };
                    loaded.afterCutAngleJudgments = new SegmentJudgment[] { DEFAULT_SEGMENT_JUDGMENT };
                    loaded.minorVersion = 1;
                    loaded.patchVersion = 0;
                    instance = loaded;
                    save();
                    return;
                }
            }
            instance = loaded;
        }

        public static void save(bool force = false)
        {
            Plugin.log("Writing file...");
            if (instance.noSerialize && !force) return;
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(instance,
                Formatting.Indented,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate }));
            Plugin.log("File written.");
        }

        public static bool validate(Config config)
        {
            if (tooNew(config))
            {
                Plugin.log("Config is for a newer version of HitScoreVisualizer!");
                return false;
            }
            bool judgmentsValid = true;
            foreach (Judgment j in config.judgments)
            {
                if (!validateJudgment(j)) judgmentsValid = false;
            }
            if (!judgmentsValid) return false;
            return true;
        }

        public static bool validateJudgment(Judgment judgment)
        {
            if (judgment.color.Length != 4)
            {
                Console.WriteLine("Judgment \"" + judgment.text + "\" with threshold " + judgment.threshold +
                    "has invalid color!");
                Console.WriteLine("Make sure to include exactly 4 numbers for each judgment's color!");
                return false;
            }
            return true;
        }

        public static bool outdated(Config config)
        {
            if (config.majorVersion < Plugin.majorVersion) return true;
            if (config.minorVersion < Plugin.minorVersion) return true;
            return false;
        }

        public static bool tooNew(Config config)
        {
            if (config.majorVersion > Plugin.majorVersion) return true;
            if (config.minorVersion > Plugin.minorVersion) return true;
            return false;
        }

        public static void resetToDefault()
        {
            instance = DEFAULT_CONFIG;
        }

        public static string floatToHexColor(float dec)
        {
            return ((int)(255f * dec)).ToString("X2");
        }

        public static void judge(FlyingScoreTextEffect text, NoteCutInfo noteCutInfo, SaberAfterCutSwingRatingCounter saberAfterCutSwingRatingCounter, ref Color color, int score)
        {
            Judgment judgment = DEFAULT_JUDGMENT;
            int index; // save in case we need to fade
            for (index = 0; index < instance.judgments.Length; index++)
            {
                Judgment j = instance.judgments[index];
                if (score >= j.threshold)
                {
                    judgment = j;
                    break;
                }
            }
            if (judgment.fade)
            {
                Judgment fadeJudgment = instance.judgments[index - 1];
                Color baseColor = toColor(judgment.color);
                Color fadeColor = toColor(fadeJudgment.color);
                float lerpDistance = Mathf.InverseLerp(judgment.threshold, fadeJudgment.threshold, score);
                color = Color.Lerp(baseColor, fadeColor, lerpDistance);
            }
            else
            {
                color = toColor(judgment.color);
            }

            //Hardcore Competetive Mode by wulkanat
            if (instance.displayMode == mode_hardcoreCompetetive)
            {
                float beforeCut = noteCutInfo.swingRating;
                float accuracy = 1f - Mathf.Clamp01(noteCutInfo.cutDistanceToCenter / 0.2f);
                float afterCut = 0f;
                if (saberAfterCutSwingRatingCounter != null)
                    afterCut = saberAfterCutSwingRatingCounter.rating;

                int beforeCutScore, accuracyScore, afterCutScore;
                int beforeMax, accuracyMax, afterMax;

                if (percentages)
                {
                    beforeMax = 100;
                    accuracyMax = 100;
                    afterMax = 100;
                }
                else
                {
                    beforeMax = 70;
                    accuracyMax = 10;
                    afterMax = 30;
                }

                beforeCutScore = Mathf.RoundToInt(beforeMax * beforeCut);
                accuracyScore = Mathf.RoundToInt(accuracyMax * accuracy);
                afterCutScore = Mathf.RoundToInt(afterMax * afterCut);

                if (beforeCutScore == beforeMax && accuracyScore == accuracyMax && afterCutScore == afterMax)
                {
                    text.text = "Perfect!";
                    return;
                }

                StringBuilder formattedBuilder = new StringBuilder();

                if (beforeCutScore == beforeMax)
                    formattedBuilder.Append("<color=#FFFFFF>P ");
                else
                    formattedBuilder.Append("<color=#" + floatToHexColor(beforeCut) + (floatToHexColor(1f - beforeCut) + "00>" + beforeCutScore + " ");

                if (accuracyScore == accuracyMax)
                    formattedBuilder.Append("<color=#FFFFFF>P ");
                else
                    formattedBuilder.Append("<color=#" + floatToHexColor(accuracy) + floatToHexColor(1f - accuracy) + "00>" + accuracyScore + " ");

                if (afterCutScore == accuracyMax)
                    formattedBuilder.Append("<color=#FFFFFF>P");
                else
                    formattedBuilder.Append("<color=#00FF00>" + afterCutScore);

                formattedBuilder.Append("\n" + "<color=#" + floatToHexColor(afterCut) + floatToHexColor(1f - afterCut) + "00>" + score);

                text.text = formattedBuilder.ToString();
            }
            else if (instance.displayMode == mode_format)
            {
                int beforeCutScore, accuracyScore, afterCutScore;

                beforeCutScore = Mathf.RoundToInt(70f * noteCutInfo.swingRating);
                float accuracy = 1f - Mathf.Clamp01(noteCutInfo.cutDistanceToCenter / 0.2f);
                accuracyScore = Mathf.RoundToInt(10f * accuracy);
                afterCutScore = 0;
                if (saberAfterCutSwingRatingCounter != null)
                {
                    afterCutScore = Mathf.RoundToInt(30f * saberAfterCutSwingRatingCounter.rating);
                }

                StringBuilder formattedBuilder = new StringBuilder();
                string formatString = judgment.text;
                int nextPercentIndex = formatString.IndexOf(format_indicator);
                while (nextPercentIndex != -1)
                {
                    formattedBuilder.Append(formatString.Substring(0, nextPercentIndex));
                    if (formatString.Length == nextPercentIndex + 1)
                    {
                        formatString += " ";
                    }
                    char specifier = formatString[nextPercentIndex + 1];

                    switch (specifier)
                    {
                        case format_beforeCutScore:
                            formattedBuilder.Append(beforeCutScore);
                            break;
                        case format_accuracyScore:
                            formattedBuilder.Append(accuracyScore);
                            break;
                        case format_afterCutScore:
                            formattedBuilder.Append(afterCutScore);
                            break;
                        case FORMAT_BEFORECUTSCORE:
                            formattedBuilder.Append(judgeSegment(beforeCutScore, instance.beforeCutAngleJudgments));
                            break;
                        case FORMAT_ACCUTRACYSCORE:
                            formattedBuilder.Append(judgeSegment(accuracyScore, instance.accuracyJudgments));
                            break;
                        case FORMAT_AFTERCUTSCORE:
                            formattedBuilder.Append(judgeSegment(afterCutScore, instance.afterCutAngleJudgments));
                            break;
                        case format_score:
                            formattedBuilder.Append(score);
                            break;
                        case format_indicatorChar:
                            formattedBuilder.Append(format_indicatorChar);
                            break;
                        case format_lineBreak:
                            formattedBuilder.Append("\n");
                            break;
                        default:
                            formattedBuilder.Append(format_indicator + specifier);
                            break;
                    }

                    formatString = formatString.Remove(0, nextPercentIndex + 2);
                    nextPercentIndex = formatString.IndexOf(format_indicator);
                }
                formattedBuilder.Append(formatString);

                text.text = formattedBuilder.ToString();
                return;
            }
            else if (instance.displayMode == mode_textOnly)
                text.text = judgment.text;
            else if (instance.displayMode == mode_numeric)
                return;
            else if (instance.displayMode == mode_scoreOnTop)
                text.text = score + "\n" + judgment.text + "\n";
            else
                text.text = judgment.text + "\n" + score + "\n";
        }

        public static Color toColor(float[] rgba)
        {
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        }

        public static string judgeSegment(int scoreForSegment, SegmentJudgment[] judgments)
        {
            if (judgments == null) return "";
            foreach(SegmentJudgment j in judgments)
            {
                if (scoreForSegment >= j.threshold) return j.text;
            }
            return "";
        }
    }
}
