using System.Collections;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    public static GameClock getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] float realSecondsPerInGameDay = 60f;
    float day;
    string time;

    void Update()
    {

        day += Time.deltaTime / realSecondsPerInGameDay;
        float dayNormalized = day % 1f;

        float rot = 360f;
        //clockHourHand.transform.eulerAngles = new Vector3(0, 0, -dayNormalized * rot);

        float hoursPerDay = 24f;
        //clockMinuteHand.transform.eulerAngles = new Vector3(0, 0, -dayNormalized * rot * hoursPerDay);

        string hours = Mathf.Floor(dayNormalized * 24f).ToString("00");
        string minutes = Mathf.Floor(((dayNormalized * hoursPerDay) % 1f) * 60f).ToString("00");

        time = hours + ":" + minutes;
    }

    public int getTime()
    {
        return int.Parse(time.Split(':')[0]);
    }
    public int getMinutes()
    {
        return int.Parse(time.Split(':')[1]);
    }
    public bool isNight()
    {
        return getTime() > 12 && getMinutes() > 0;
    }
}