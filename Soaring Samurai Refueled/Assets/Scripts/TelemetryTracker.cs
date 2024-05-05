using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TelemetryTracker : MonoBehaviour
{

    class PlayerTrackingData
    {
        public PlayerTrackingData(PlayerTrackingData other)
        {
            MaxLinearVelocity = other.MaxLinearVelocity;
            MaxLinearAcceleration = other.MaxLinearAcceleration;
            MaxLinearJerk = other.MaxLinearJerk;

            MaxAngularVelocity = other.MaxAngularVelocity;
            MaxAngularAcceleration = other.MaxAngularAcceleration;
            MaxAngularJerk = other.MaxAngularJerk;
        }

        public PlayerTrackingData()
        {
            MaxLinearVelocity = Vector2.zero;
            MaxLinearAcceleration = Vector2.zero;
            MaxLinearJerk = Vector2.zero;

            MaxAngularVelocity = 0.0f;
            MaxAngularAcceleration = 0.0f;
            MaxAngularJerk = 0.0f;
        }
        public Vector2 MaxLinearVelocity = Vector2.zero;
        public Vector2 MaxLinearAcceleration = Vector2.zero;
        public Vector2 MaxLinearJerk = Vector2.zero;

        public float MaxAngularVelocity = 0.0f;
        public float MaxAngularAcceleration = 0.0f;
        public float MaxAngularJerk = 0.0f;

        public void ResetVariables()
        {

            MaxLinearVelocity = Vector2.zero;
            MaxLinearAcceleration = Vector2.zero;
            MaxLinearJerk = Vector2.zero;

            MaxAngularVelocity = 0.0f;
            MaxAngularAcceleration = 0.0f;
            MaxAngularJerk = 0.0f;
        }

    }

    class WritingTable
    {
        SortedDictionary<string, float> mWriteValues = new SortedDictionary<string, float>();

        // Adds the given value to the currently stored value for the given name
        // If value does not exist, it is created
        public void AddToItem(string name, float value)
        {
            if (mWriteValues.ContainsKey(name) == false)
            {
                mWriteValues.Add(name, 0.0f);
            }

            mWriteValues[name] += value;
        }

        public string GetHeaderLine()
        {
            string header = "";

            foreach (var writeItemPair in mWriteValues)
            {
                header += writeItemPair.Key + ",";
            }

            header = header.Substring(0, header.Length - 1); // Remove the extra comma at the end
            header += "\n";

            return header;
        }

        public string GetValuesLine()
        {
            string header = "";

            foreach (var writeItemPair in mWriteValues)
            {
                header += writeItemPair.Value.ToString() + ",";
            }

            header = header.Substring(0, header.Length - 1); // Remove the extra comma at the end
            header += "\n";

            return header;
        }

        public bool IsEmpty()
        {
            return mWriteValues.Count == 0;
        }
    }


    class PlayerTrackingPackage
    {
        public PlayerTrackingPackage()
        {
            mPhysicsData = new PlayerTrackingData();
            mTimeStamp = 0.0f;
        }
        public PlayerTrackingPackage(PlayerTrackingPackage other)
        {
            mPhysicsData = new PlayerTrackingData(other.mPhysicsData);
            mTimeStamp = other.mTimeStamp;
        }
        public PlayerTrackingData mPhysicsData;
        public float mTimeStamp;

    }

    public StreamWriter mDataStream;

    public static TelemetryTracker Instance;

    PlayerTrackingPackage mCurrPlayerData = new PlayerTrackingPackage();

    List<PlayerTrackingPackage> mPlayerTrackingList = new List<PlayerTrackingPackage>();

    WritingTable mSimTrackingValues = new WritingTable();

    float mStartTime = 0.0f;

    float mPlayerDataSaveDelay = 1.0f;
    float mPlayerDataSaveTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        mStartTime = Time.realtimeSinceStartup;

        mPlayerDataSaveTimer = mPlayerDataSaveDelay;

        SimManager.Instance.GameEnd += EndOfGameWrite;

        // Singleton, keep around until game shutdown
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        mPlayerDataSaveTimer -= Time.deltaTime;

        if (mPlayerDataSaveTimer <= 0.0f)
        {
            // Saves current player data
            mCurrPlayerData.mTimeStamp = Time.realtimeSinceStartup - mStartTime;
            PlayerTrackingPackage currPackage = new PlayerTrackingPackage(mCurrPlayerData);
            mPlayerTrackingList.Add(currPackage);
            mCurrPlayerData.mPhysicsData.ResetVariables();

            mPlayerDataSaveTimer = mPlayerDataSaveDelay;
        }

    }

    // Tracking functions //////////////////////////////////////////////////////////////////////////////////
    public void TrackPlayerLinearJerk(Vector2 currJerk)
    {
        if (mCurrPlayerData.mPhysicsData.MaxLinearJerk.magnitude < currJerk.magnitude)
        {
            mCurrPlayerData.mPhysicsData.MaxLinearJerk = currJerk;
        }
    }
    public void TrackPlayerLinearAcceleration(Vector2 currAcceleration)
    {
        if (mCurrPlayerData.mPhysicsData.MaxLinearAcceleration.magnitude < currAcceleration.magnitude)
        {
            mCurrPlayerData.mPhysicsData.MaxLinearAcceleration = currAcceleration;
        }
    }
    public void TrackPlayerLinearVelocity(Vector2 currVelocity)
    {
        if (mCurrPlayerData.mPhysicsData.MaxLinearVelocity.magnitude < currVelocity.magnitude)
        {
            mCurrPlayerData.mPhysicsData.MaxLinearVelocity = currVelocity;
        }
    }

    public void TrackPlayerAngularJerk(float currJerk)
    {
        if (Mathf.Abs(mCurrPlayerData.mPhysicsData.MaxAngularJerk) < Mathf.Abs(currJerk))
        {
            mCurrPlayerData.mPhysicsData.MaxAngularJerk = currJerk;
        }
    }

    public void TrackPlayerAngularAcceleration(float currAcceleration)
    {
        if (Mathf.Abs(mCurrPlayerData.mPhysicsData.MaxAngularAcceleration) < Mathf.Abs(currAcceleration))
        {
            mCurrPlayerData.mPhysicsData.MaxAngularAcceleration = currAcceleration;
        }
    }

    public void TrackPlayerAngularVelocity(float currVelocity)
    {
        if (Mathf.Abs(mCurrPlayerData.mPhysicsData.MaxAngularVelocity) < Mathf.Abs(currVelocity))
        {
            mCurrPlayerData.mPhysicsData.MaxAngularVelocity = currVelocity;
        }
    }

    
    public void AddToSimStat(string name, float value)
    {
        mSimTrackingValues.AddToItem(name, value);
    }
    // ////////////////////////////////////////////////////////////////////////////////////////////
    //public void WriteLine(string line)
    //{
    //    mDataStream.WriteLine(line);
    //}

    //public void Close()
    //{
    //    mDataStream.Close();
    //}

    void EndOfGameWrite()
    {
        mDataStream = new StreamWriter("SoaringSamuraiData.csv", true);

        mDataStream.WriteLine("Player Data\n");
        mDataStream.WriteLine("Timestamp, Linear Jerk,, Linear Acceleration,, Linear Velocity,, Angular Jerk, Angular Acceleration, Angular Velocity\n");
        foreach (var playerSnapshot in mPlayerTrackingList)
        {
            mDataStream.WriteLine(playerSnapshot.mTimeStamp + "," +
                                  playerSnapshot.mPhysicsData.MaxLinearJerk.x.ToString() + "," + playerSnapshot.mPhysicsData.MaxLinearJerk.y.ToString() + "," +
                                  playerSnapshot.mPhysicsData.MaxLinearAcceleration.x.ToString() + "," + playerSnapshot.mPhysicsData.MaxLinearAcceleration.y.ToString() + "," +
                                  playerSnapshot.mPhysicsData.MaxLinearVelocity.x.ToString() + "," + playerSnapshot.mPhysicsData.MaxLinearVelocity.y.ToString() + "," +
                                  playerSnapshot.mPhysicsData.MaxAngularJerk.ToString() + "," +
                                  playerSnapshot.mPhysicsData.MaxAngularAcceleration.ToString() + "," +
                                  playerSnapshot.mPhysicsData.MaxAngularVelocity.ToString());
        }
        mDataStream.Write("\n");

        mDataStream.WriteLine("Sim Data\n");
        mDataStream.WriteLine(mSimTrackingValues.GetHeaderLine());
        mDataStream.WriteLine(mSimTrackingValues.GetValuesLine());
        mDataStream.Write("\n");



        mDataStream.Close();
    }
}
