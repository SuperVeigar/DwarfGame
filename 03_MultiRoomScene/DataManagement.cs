using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


// 호스트에서 json을 이용한 참가자 정보 저장 및 불러오기.
// 호스트만 참가자 정보 참조하여 케릭터 instantiate 하기 위함. 위조 및 변조 방지
[System.Serializable]
public class SaveData
{
    public int m_dataSize = 0;
    public string[] m_userID = new string[4];
    public string[] m_nickname = new string[4];
    public PlayerColor[] m_playerColor = new PlayerColor[4];
    public PlayerWeapon[] m_playerWeapon = new PlayerWeapon[4];

    public void Init()
    {
        for (int i = 0; i<4; i++)
        {
            m_userID[i] = "Defualt";
            m_nickname[i] = "Defualt";
            m_playerColor[i] = (PlayerColor)((int)PlayerColor.start + 1);
            m_playerWeapon[i] = (PlayerWeapon)((int)PlayerWeapon.start + 1);
        }
    }
    public void AddSize(int addNum)
    {
        m_dataSize += addNum;

        if(m_dataSize>=4)
        {
            m_dataSize = 4;
        }
    }
    public void Copy(SaveData data)
    {

    }
}


public class DataManagement : MonoBehaviour
{
    public SaveData m_saveData { get; private set; }
        
    private string m_savePath;

    private void Awake()
    {
        m_savePath = Path.Combine(Application.dataPath, "saveData.json");
        m_saveData = new SaveData();
        m_saveData.Init();
    }
    // Start is called before the first frame update
    void Start()
    {        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddJson(int num, string userID, string nickname, PlayerColor playerColor, PlayerWeapon playerWeapon)
    {
        m_saveData.m_userID[num] = userID;
        m_saveData.m_nickname[num] = nickname;
        m_saveData.m_playerColor[num] = playerColor;
        m_saveData.m_playerWeapon[num] = playerWeapon;
        m_saveData.AddSize(1);
    }

    public void SaveJson()
    {
        string json = JsonUtility.ToJson(m_saveData, true);

        File.WriteAllText(m_savePath, json);
    }

    public SaveData LoadJson()
    {
        if (File.Exists(m_savePath))
        {
            string loadJason = File.ReadAllText(m_savePath);

            m_saveData = JsonUtility.FromJson<SaveData>(loadJason);

            return m_saveData;
        }
        return null;
    }
}
