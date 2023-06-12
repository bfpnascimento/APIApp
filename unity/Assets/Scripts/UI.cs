using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // holds all the segments in a vertical layout
    public RectTransform container;

    // segment prefab to instantiate
    public GameObject segmentPrefab;
    
    // list of all available segments
    private List<GameObject> segments = new List<GameObject>();

    [Header("Info Dropdown")]
    // info dropdown object
    public RectTransform infoDropdown;

    // text showing time, event type, etc
    public TextMeshProUGUI infoDropdownText;
    
    // text showing the event address
    public TextMeshProUGUI infoDropdownAddressText;

    // instance
    public static UI instance;

    void Awake ()
    {
        // set the instance to this script
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // preload 10 segments
        PreLoadSegments(10);

        // get and set the data
        AppManager.instance.StartCoroutine("GetData", "abc");
    }

    // instantiates a set number of segments to use later on
    void PreLoadSegments (int amount)
    {
        // instantiate 'amount' number of new segments
        for (int x = 0; x < amount; ++x)
            CreateNewSegment();
    }

    // creates a new segment and returns it
    GameObject CreateNewSegment ()
    {
        // instantiate and setup the segment
        GameObject segment = Instantiate(segmentPrefab);
        segment.transform.SetParent(container.transform);
        
        // add OnClick event listener to the button
        segment.GetComponent<Button>().onClick.AddListener(() => { OnShowMoreInfo(segment); });
        
        // deactivate it by default
        segment.SetActive(false);

        segments.Add(segment);

        return segment;
    }

    // gets the JSON result and displays them on the screen with their respective segments
    public void SetSegments (JSONNode records)
    {
        DeactivateAllSegments();
        
        // loop through all records
        for(int x = 0; x < records.Count; ++x)
        {
            // create a new segment if we don't have enough
            GameObject segment = x < segments.Count ? segments[x] : CreateNewSegment();
            segment.SetActive(true);
            
            // get the location and date texts
            TextMeshProUGUI locationText = segment.transform.Find("LocationText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = segment.transform.Find("DateText").GetComponent<TextMeshProUGUI>();
            
            // set them
            locationText.text = records[x]["Suburb"];
            dateText.text = GetFormattedDate(records[x]["Display Date"]);
        }
        
        // set the container size to clamp to the segments
        container.sizeDelta = new Vector2(container.sizeDelta.x, GetContainerHeight(records.Count));
    }

    // deactivates all of the segment objects
    void DeactivateAllSegments ()
    {
        // loop through all segments and deactivate them
        foreach (GameObject segment in segments)
            segment.SetActive(false);
    }

    // returns a date that is formatted from the raw json data
    string GetFormattedDate (string rawDate)
    {
        // convert the raw date to a DateTime object
        DateTime date = DateTime.Parse(rawDate);
        
        // build a "[day]/[month]/[year]" formatted date and return it
        return string.Format("{0}/{1}/{2}", date.Day, date.Month, date.Year);
    }

    // returns a height to make the container so it clamps to the size of all segments
    float GetContainerHeight (int count)
    {
        float height = 0.0f;
        
        // include all segment heights
        height += count * (segmentPrefab.GetComponent<RectTransform>().sizeDelta.y + 1);
        
        // include the spacing between segments
        height += count * container.GetComponent<VerticalLayoutGroup>().spacing;
        
        // include the info dropdown height
        height += infoDropdown.sizeDelta.y;
        
        return height;
    }

    // called when the input field has been submitted
    public void OnSearchBySuburb (TextMeshProUGUI input)
    {
        DeactivateAllSegments();

        // remove ends and breaklines
        string result = input.text.Replace("\u200B", "");

        // get and set the data
        AppManager.instance.StartCoroutine("GetData", result);
        
        // disable the info dropdown
        infoDropdown.gameObject.SetActive(false);
    }
    
    // called when the user selects a segment - toggles the dropdown
    public void OnShowMoreInfo (GameObject segmentObject)
    {
        // get the index of the segment
        int index = segments.IndexOf(segmentObject);
        
        // if we're pressing the segment that's already open, close the dropdown
        if(infoDropdown.transform.GetSiblingIndex() == index + 1 && infoDropdown.gameObject.activeInHierarchy)
        {
            infoDropdown.gameObject.SetActive(false);
            return;
        }

        infoDropdown.gameObject.SetActive(true);
        
        // get only the records
        JSONNode records = AppManager.instance.jsonResult["result"]["records"];
        
        // set the dropdown to appear below the selected segment
        infoDropdown.transform.SetSiblingIndex(index + 1);
        
        // set dropdown info text
        infoDropdownText.text = "Starts at " + GetFormattedTime(records[index]["Times(s)"]);
        infoDropdownText.text += "\n" + records[index]["Event Type"] + " Event";
        infoDropdownText.text += "\n" + records[index]["Display Type"];
        
        // set dropdown address text
        if (records[index]["Display Address"].ToString().Length > 2)
            infoDropdownAddressText.text = records[index]["Display Address"];
        else
            infoDropdownAddressText.text = "Address not specified";
    }

    // converts 24 hour time to 12 hour time
    // e.g. 19:30 = 7:30 PM
    string GetFormattedTime (string rawTime)
    {
        // get the hours and minutes from the raw time
        string[] split = rawTime.Split(":"[0]);
        int hours = int.Parse(split[0]);
        
        // converts it to "[hours]:[mins] (AM / PM)"
        return string.Format("{0}:{1} {2}", hours > 12 ? hours - 12 : hours, split[1], hours > 12 ? "PM" : "AM");
    }
}
