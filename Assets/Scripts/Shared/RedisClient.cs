using System.Collections.Generic;
using TeamDev.Redis;
using UnityEngine;

public class RedisClient
{
    public delegate void receive(ushort message_id, string message);
    public delegate void channel_subscribed(string channel);

    RedisDataAccessProvider redis;

    public event receive Receive;
    public event channel_subscribed Channel_Subscribed;

    public RedisClient(string host, int port, string database, string username)
    {
        redis = new RedisDataAccessProvider();
        redis.Configuration.Host = host;
        redis.Configuration.Port = port;
        redis.Configuration.DataBase = database;
        redis.Configuration.UserName = username;

        redis.ChannelSubscribed += new ChannelSubscribedHandler(channelSubscribed);
        redis.MessageReceived += new MessageReceivedHandler(messageReceived);
    }

    public void Publish(string channel, string message, ushort message_id)
    {
        redis.Messaging.Publish(channel, message_id + ";" + message);
    }
    public void Subscribe(params string[] channels)
    {
        redis.Messaging.Subscribe(channels);
    }

    void channelSubscribed(string channelname)
    {
        Channel_Subscribed(channelname);

        Debug.Log(channelname);
    }
    void messageReceived(string channelname, string message)
    {
        string[] splitted_message = message.Split(';');

        ushort message_id = ushort.Parse(splitted_message[0]);

        string[] temp = splitted_message;
        splitted_message = new string[temp.Length - 1];
        for (int i = 1; i < temp.Length; i++)
            splitted_message[i - 1] = temp[i];
        message = "";
        foreach (var i in splitted_message)
            message += i;

        Receive(message_id, message);

        Debug.Log(string.Format("{0} - {1} ", channelname, message));
    }

    //the dictionary will be: Field, Value ... Field, Value
    public void Write(string key, Dictionary<string, string> values)
    {
        redis.Hash[key].Set(values);
    }
    public void Write(string key, string field, string value)
    {
        redis.Hash[key].Set(field, value);
    }
    public string Read(string key, string field)
    {
        string data = "";

        if (keyExists(key))
            data = redis.Hash[key].Get(field);

        return data;
    }
    public void Delete(string key, string field)
    {
        redis.Hash[key].Delete(field);
    }
    public Dictionary<string, string> ReadValues(string key)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();

        if (keyExists(key))
            foreach (var i in redis.Hash[key].Items)
                dic.Add(i.Key, i.Value);

        return dic;
    }

    public bool keyExists(string key)
    {
        return redis.Key.Exists(key);
    }

    public void Write(string key, string field, int value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, float value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, char value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, bool value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, double value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, long value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, short value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, ushort value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, uint value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, ulong value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, byte value)
    {
        Write(key, field, value.ToString());
    }
    public void Write(string key, string field, byte[] value)
    {
        string _value = "";
        foreach (var i in value)
            _value += i.ToString() + ";";

        Write(key, field, value);
    }

    public int ReadInt(string key, string field)
    {
        return int.Parse(Read(key, field));
    }
    public float ReadFloat(string key, string field)
    {
        return float.Parse(Read(key, field));
    }
    public char ReadChar(string key, string field)
    {
        return char.Parse(Read(key, field));
    }
    public bool ReadBool(string key, string field)
    {
        return bool.Parse(Read(key, field));
    }
    public double ReadDouble(string key, string field)
    {
        return double.Parse(Read(key, field));
    }
    public long ReadLong(string key, string field)
    {
        return long.Parse(Read(key, field));
    }
    public short ReadShort(string key, string field)
    {
        return short.Parse(Read(key, field));
    }
    public ushort ReadUShort(string key, string field)
    {
        return ushort.Parse(Read(key, field));
    }
    public uint ReadUInt(string key, string field)
    {
        return uint.Parse(Read(key, field));
    }
    public ulong ReadULong(string key, string field)
    {
        return ulong.Parse(Read(key, field));
    }
    public byte ReadByte(string key, string field)
    {
        return byte.Parse(Read(key, field));
    }
    public byte[] ReadBytes(string key, string field)
    {
        string value = Read(key, field);
        string[] value_splitted = value.Split(';');
        byte[] _value = new byte[value_splitted.Length];

        for (int i = 0; i < value.Length; i++)
            _value[i] = byte.Parse(value_splitted[i]);

        return _value;
    }
}