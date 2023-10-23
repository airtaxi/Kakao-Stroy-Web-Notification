﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static KakaoStroyWebNotification.Api.ApiHandler.DataType;

namespace KakaoStroyWebNotification.Api;

public static class Utils
{

    private static readonly DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long ToUnixTime(DateTime date)
    {
        return Convert.ToInt64((date - epoch).TotalSeconds);
    }

    public static string GetStringFromQuoteData(List<QuoteData> datas, bool preserveQuote)
    {
        StringBuilder sb = new();
        foreach (var data in datas)
        {
            if (preserveQuote)
            {
                if (data.type.Equals("profile"))
                {
                    sb.Append("{!{" + JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }) + "}!}");
                }
                else if (data.type.Equals("emoticon"))
                {
                    sb.Append("{!{" + JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }) + "}!}");
                }
                else
                    sb.Append(data.text);
            }
            else
                sb.Append(data.text);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Spaghetti code, should be refactored.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="escapeHashtag"></param>
    /// <returns></returns>
    public static List<QuoteData> GetQuoteDataFromString(string text, bool escapeHashtag = false)
    {
        text = text.Replace("\r\n", "\n");
        text = text.Replace("\r", "\n");
        string[] fragmentBases = text.Split(new string[] { "{!{" }, StringSplitOptions.None);
        var returnData = new List<QuoteData>();
        int count = 0;
        foreach (string fragmentBase in fragmentBases)
        {
            if (count % 2 == 0)
            {
                string str = fragmentBase.Contains("}!}") ? fragmentBase.Split(new string[] { "}!}" }, StringSplitOptions.None)[1] : fragmentBase;
                if (str.Contains('#') && !escapeHashtag)
                {
                    string[] rawStr = str.Split(new string[] { "#" }, StringSplitOptions.None);
                    if (rawStr[0].Length > 0)
                    {
                        returnData.Add(new QuoteData()
                        {
                            type = "text",
                            text = rawStr[0]
                        });
                    }
                    for (int i = 1; i < rawStr.Length; i++)
                    {
                        string strNow = rawStr[i];
                        int splitCounter = Math.Min(strNow.IndexOf(" "), strNow.IndexOf("\n"));
                        if (splitCounter >= 0)
                        {
							string hashTag = strNow[..splitCounter];
                            string otherStr = strNow[splitCounter..];
                            if (hashTag.Length > 0)
                            {
                                returnData.Add(new QuoteData()
                                {
                                    type = "hashtag",
                                    hashtag_type = "",
                                    hashtag_type_id = "",
                                    text = "#" + hashTag
                                });
                            }
                            else
                            {
                                returnData.Add(new QuoteData()
                                {
                                    type = "text",
                                    text = "#"
                                });
                            }
                            if (otherStr.Length > 0)
                            {
                                returnData.Add(new QuoteData()
                                {
                                    type = "text",
                                    text = otherStr
                                });
                            }
                        }
                        else
                        {
                            returnData.Add(new QuoteData()
                            {
                                type = "hashtag",
                                hashtag_type = "",
                                hashtag_type_id = "",
                                text = "#" + strNow
                            });
                        }
                    }
                }
                else
                {
                    var quoteData = new QuoteData()
                    {
                        type = "text",
                        text = str
                    };
                    returnData.Add(quoteData);
                }
                count++;
            }
            else
            {
                string[] strs = fragmentBase.Split(new string[] { "}!}" }, StringSplitOptions.None);
                string jsonStr = strs[0];
                var quoteData = JsonConvert.DeserializeObject<QuoteData>(jsonStr);
                count++;
                returnData.Add(quoteData);
                if (strs.Length == 2)
                {
                    var quoteData2 = new QuoteData()
                    {
                        type = "text",
                        text = strs[1]
                    };
                    returnData.Add(quoteData2);
                    count++;
                }
            }
        }
        return returnData;
    }

    public static string GetTimeString(DateTime created_at)
    {
        int offset = DateTimeOffset.Now.Offset.Hours;
        string dateText = created_at.AddHours(offset).ToString();
        var diffTime = DateTime.Now.Subtract(created_at.AddHours(offset));
        if (diffTime.TotalSeconds < 60)
        {
            dateText = "방금 전";
        }
        else if (diffTime.TotalMinutes < 60)
        {
            dateText = ((int)diffTime.TotalMinutes).ToString() + "분 전";
        }
        else if (diffTime.TotalHours < 24)
        {
            dateText = ((int)diffTime.TotalHours).ToString() + "시간 전";
        }
        return dateText;
    }
}
