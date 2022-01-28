using System;
using System.Collections;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class Utils : MonoBehaviour
{
    public static Utils getInstance;
    void Awake()
    {
        getInstance = this;

        buttons = FindObjectsOfType<Button>();
    }

    public void Exit()
    {
        Client.getInstance.CloseConnection();
        Application.Quit();
    }
    public void Exit(float seconds)
    {
        StopCoroutine(ExitTask(seconds));
        StartCoroutine(ExitTask(seconds));
    }
    IEnumerator ExitTask(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Exit();
    }

    public string GetPing(string ip)
    {
        using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
        {
            return ping.Send(ip).RoundtripTime.ToString();
        }
    }

    public string GetRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    }

    public static double MinutesToMiliseconds(int minutes)
    {
        return TimeSpan.FromMinutes(minutes).TotalMilliseconds;
    }

    public static void Swap(ref object obj1, ref object obj2)
    {
        object temp = obj1;
        obj1 = obj2;
        obj2 = temp;
    }

    public static PhysicalAddress GetLocalMacAddress()
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                nic.OperationalStatus == OperationalStatus.Up)
            {
                return nic.GetPhysicalAddress();
            }
        }
        return null;
    }
    public static string GetLocalMacAddressToString()
    {
        return GetLocalMacAddress().ToString();
    }

    public static long LongRandom(long min, long max, System.Random rnd)
    {
        long result = rnd.Next((Int32)(min >> 32), (Int32)(max >> 32));
        result = (result << 32);
        result = result | (long)rnd.Next((Int32)min, (Int32)max);
        return result;
    }

    [SerializeField] GameObject loadingPopup;
    public Button[] buttons;
    bool loading = false;
    public void Loading(bool value)
    {
        loading = value;

        foreach (var i in buttons)
            i.gameObject.SetActive(!value);

        loadingPopup.SetActive(value);
    }
}