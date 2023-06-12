using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class AppManager : MonoBehaviour
{
    public enum Duration
    {
        Today = 0,
        Week = 1,
        Month = 2,
        All = 3
    }

    // API url
    public string url;

    // resulting JSON from an API request
    public JSONNode jsonResult;

    // instance
    public static AppManager instance;

    void Awake ()
    {
        // set the instance to be this script
        instance = this;
    }

    // called when a duration button is pressed
    // filters the list based on the max time length given
    public void FilterByDuration (int durIndex)
    {
        // get the duration enum from the index
        Duration dur = (Duration)durIndex;

        // get an array of the records
        JSONArray records = jsonResult["result"]["records"].AsArray;

        // create the max date
        DateTime maxDate = new DateTime();

        // set the max date depending on the duration
        switch(dur)
        {
            case Duration.Today:
                maxDate = DateTime.Now.AddDays(1);
                break;
            case Duration.Week:
                maxDate = DateTime.Now.AddDays(7);
                break;
            case Duration.Month:
                maxDate = DateTime.Now.AddMonths(1);
                break;
            case Duration.All:
                maxDate = DateTime.MaxValue;
                break;
        }

        // create a new JSONArray to hold all the filtered records
        JSONArray filteredRecords = new JSONArray();

        // loop through all the records and add the ones within the duration, to the filtered records
        for(int x = 0; x < records.Count; ++x)
        {
            // get the record's display date
            DateTime recordDate = DateTime.Parse(records[x]["Display Date"]);

            // if the record's display date is before the max date, add it to the filtered records
            if (recordDate.Ticks < maxDate.Ticks)
                filteredRecords.Add(records[x]);
        }

        // display the results on screen
        UI.instance.SetSegments(filteredRecords);
    }

    // sends an API request - returns a JSON file
    IEnumerator GetData (string location)
    {
        // create the web request and download handler
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query
        webReq.url = string.Format("{0}&q={1}", url, location);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array and wait for a returning result
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);

        // display the results on screen
        UI.instance.SetSegments(jsonResult["result"]["records"]);
    }
}
