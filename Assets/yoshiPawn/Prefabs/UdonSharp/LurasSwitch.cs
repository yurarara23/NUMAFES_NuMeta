
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class LurasSwitch : UdonSharpBehaviour
{
    [Header("------------------------------------------------------------------")]
    [Header("■■■ 機能切り替えスライダー／Function switching slider ■■■")]
    [Header("------------------------------------------------------------------")]
    [Header("[0]トグルスイッチ(反転)／ToggleSwitch(Invert)")]
    [Header("[1] シーケンススイッチ／Sequence switch")]
    [Header("[2] 位置リセットスイッチ／Position Reset switch (Global)")]

    [Space(20)]

    [Range(0, 2)] public int switchID;

    [Space(10)]

    [SerializeField] private bool isGlobal = false;

    [Space(10)]

    [Header("------------------------------------------------------------------")]
    [Header("■■■ 対象オブジェクト設定／Target Object Setting ■■■")]
    [Header("------------------------------------------------------------------")]

    [Header("ターゲットオブジェクトのsizeに数を入れて切り替えたいオブジェクトをドラッグ＆ドロップ")]
    [Header("Enter a number in size and drag and drop the object you want to switch")]


    [Space(10)]

    [SerializeField] private GameObject[] targetObject;




    //LocalMode(SequenceSwitch用)
    [HideInInspector][SerializeField] private int localActiveObjectIndex = 0;

    //GlobalMode(SequenceSwitch用)
    [HideInInspector][UdonSynced] public int globalActiveObjectIndex = 0;

    //GlobalMode(ToggleSwitch用)
    [HideInInspector][UdonSynced] public bool isTapGlobal;



    [Header("Settings")]
    [SerializeField][HideInInspector] private bool isTap = false;

    [Header("------------------------------------------------------------------")]
    [Header("■■■ 永続モード／Persistance Mode ■■■")]
    [Header("------------------------------------------------------------------")]
    [Header("永続モードを使う場合はチェックを入れてください / Check if you want to use Persistance Mode")]
    [Header("インスタンス再入場時、最後の状態を再現します / Recreate the last state when re-entering the instance")]
    [Header("isGlobalがtrueの場合は機能しません / Does not work if isGlobal is true")]
    [Space(10)]
    [Header("永続モードを使う／Use Persistance Mode")]
    [SerializeField] private bool usePersist = false;
    [Header("------------------------------------------------------------------")]
    [Header("[0]トグルスイッチ使用時／ToggleSwitch")]
    [Header("保存されるユニークな名前を設定してください／Set a unique name to be saved")]
    [SerializeField] private string TOGGLE_KEY = "TOGGLE_1";
    [Header("------------------------------------------------------------------")]
    [Header("[1] シーケンススイッチ使用時／Sequence switch")]
    [Header("保存されるユニークな名前を設定してください／Set a unique name to be saved")]
    [SerializeField] private string SEQUENCE_KEY = "SEQUENCE_1";

    [Header("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■")]

    //PositionReset用
    private Vector3[] defaultPosition;
    private Quaternion[] defaultRotation;
    private Rigidbody[] targetRigidbody;

    // 各オブジェクトの初期状態を保持するための配列
    private bool[] initialStates;


    void Start()
    {
        switch (switchID)
        {
            case 0:
                //ToggleSwitch
                // 配列を初期化
                initialStates = new bool[targetObject.Length];
                for (int i = 0; i < targetObject.Length; i++)
                {
                    if (targetObject[i] != null)
                    {
                        initialStates[i] = targetObject[i].activeSelf;
                    }
                }
                break;
            case 1:
                //SequenceSwitch
                break;
            case 2:
                //PositionResetSwitch
                defaultPosition = new Vector3[targetObject.Length];
                defaultRotation = new Quaternion[targetObject.Length];
                targetRigidbody = new Rigidbody[targetObject.Length];

                for (int i = 0; i < targetObject.Length; i++)
                {
                    if (targetObject[i] != null)
                    {
                        defaultPosition[i] = targetObject[i].transform.position;
                        defaultRotation[i] = targetObject[i].transform.rotation;
                        targetRigidbody[i] = targetObject[i].GetComponent<Rigidbody>();
                    }
                }
                break;
        }
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        switch (switchID)
        {
            case 0:
                if (usePersist)    // パーシスタントデータを使う場合
                {
                    if (!player.isLocal) return;

                    if (!isGlobal)
                    {
                        //Localの場合
                        if (PlayerData.HasKey(player, TOGGLE_KEY))
                        {
                            //パーシスタントデータにBoolを読み込む
                            isTap = PlayerData.GetBool(player, TOGGLE_KEY);
                            ToggleObjectLocal(isTap);
                        }
                    }
                    else
                    {
                        //Globalの場合は機能しない
                    }
                }

                break;

            case 1:
                if (usePersist)    // パーシスタントデータを使う場合
                {
                    if (!player.isLocal) return;

                    if (!isGlobal)
                    {
                        //Localの場合
                        if (PlayerData.HasKey(player, SEQUENCE_KEY))
                        {
                            //パーシスタントデータにintを読み込む
                            localActiveObjectIndex = PlayerData.GetInt(player, SEQUENCE_KEY);
                            SwitchActiveObjectLocal();       //activeObjectIndexをセットして反映させる(Local)
                        }
                    }
                    else
                    {
                        //Globalの場合は機能しない
                    }
                }
                break;
        }
    }

    //Deserialize
    public override void OnDeserialization()
    {
        switch (switchID)
        {
            case 0:
                if (isGlobal)
                {
                    ToggleObjectLocal(isTapGlobal);
                }
                break;

            case 1:
                if (isGlobal)
                {
                    SwitchActiveObjectGlobal();
                }
                break;
        }
    }


    //インタラクト
    public override void Interact()
    {
        switch (switchID)
        {
            case 0:
                //ToggleSwitch Local
                if (!usePersist)    //パーシスタントデータを使わない場合
                {
                    if (!isGlobal)
                    {
                        isTap = !isTap; //押された状態かどうかを反転
                        //ToggleSwitch LocalMode
                        ToggleObjectLocal(isTap);    //ObjectIndexのオンオフを反転させる
                    }
                    else
                    {
                        //ToggleSwitch Global

                        if (!Networking.IsOwner(gameObject))
                        {
                            //オーナーが自分でない場合、自分にオーナーを設定
                            Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        }

                        //同期変数のBoolを反転
                        isTapGlobal = !isTapGlobal;

                        //オブジェクトのアクティブを切り替え
                        ToggleObjectLocal(isTapGlobal);    //ObjectIndexのオンオフを反転させる
                        RequestSerialization(); //同期をリクエスト
                    }
                }
                else
                {
                    //パーシスタントデータを使う場合

                    if (!isGlobal)
                    {
                        //ToggleSwitch Local Persistent
                        //オブジェクトのアクティブを反転させる
                        isTap = !isTap; //押された状態かどうかを反転

                        //パーシスタントデータにBoolを保存
                        PlayerData.SetBool(TOGGLE_KEY, isTap);

                        ToggleObjectLocal(isTap);
                    }
                    else
                    {
                        //Globalの場合はPersistantは機能しない（!usePersist/Globalと同じになる）

                        if (!Networking.IsOwner(gameObject))
                        {
                            //オーナーが自分でない場合、自分にオーナーを設定
                            Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        }

                        //同期変数のBoolを反転
                        isTapGlobal = !isTapGlobal;

                        //オブジェクトのアクティブを切り替え
                        ToggleObjectLocal(isTapGlobal);    //ObjectIndexのオンオフを反転させる
                        RequestSerialization(); //同期をリクエスト
                    }
                }
                break;

            case 1:
                if (!usePersist)    //パーシスタントデータを使わない場合
                {
                    if (!isGlobal)
                    {
                        //SequenceSwitch Local
                        NextObjectIndexLocal();     //次のObjectIndexに切り替える(Local)
                    }
                    else
                    {
                        //SequenceSwitch Global
                        //Global
                        if (!Networking.IsOwner(gameObject))
                        {
                            Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        }
                        NextObjectIndexGlobal();    //次のObjectIndexに切り替える(Global)
                    }
                }
                else
                {
                    if (!isGlobal)
                    {
                        //SequenceSwitch Local Persistent
                        //次のObjectIndexに切り替える(Local)

                        NextObjectIndexLocal();

                        //パーシスタントデータにintを保存
                        PlayerData.SetInt(SEQUENCE_KEY, localActiveObjectIndex);
                    }
                    else
                    {
                        //Globalの場合はPersistantは機能しない（!usePersist/Globalと同じになる）

                        if (!Networking.IsOwner(gameObject))
                        {
                            Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        }
                        NextObjectIndexGlobal();    //次のObjectIndexに切り替える(Global)
                    }
                }
                break;

            case 2:
                //SwitchType -Position Reset Switch-
                for (int i = 0; i < targetObject.Length; i++)
                {
                    Networking.SetOwner(Networking.LocalPlayer, targetObject[i]);
                }

                for (int i = 0; i < targetObject.Length; i++)
                {
                    if (targetObject[i] != null)
                    {
                        if (defaultPosition[i] != null)
                        {
                            targetObject[i].transform.position = defaultPosition[i];
                            targetObject[i].transform.rotation = defaultRotation[i];
                            targetRigidbody[i].Sleep();
                        }
                    }
                }
                break;
        }




    }

    public void ToggleObjectLocal(bool invert)     //オブジェクトのアクティブを切り替え
    {
        for (int x = 0; x < targetObject.Length; x++)
        {
            if (targetObject[x] != null) // 配列内のNullチェック
            {
                if (invert)
                {
                    // invertがtrueの場合、オブジェクトのアクティブ状態を反転させる
                    targetObject[x].SetActive(!initialStates[x]);
                }
                else
                {
                    // invertがfalseの場合、オブジェクトを元の状態に戻す
                    targetObject[x].SetActive(initialStates[x]);
                }
            }
        }
    }

    private void AddActiveObjectIndexLocal()  //activeObjectIndexに１を足す(Local)
    {
        localActiveObjectIndex = localActiveObjectIndex + 1;

        if (localActiveObjectIndex >= targetObject.Length)
        {
            localActiveObjectIndex = 0;
        }
    }

    private void AddActiveObjectIndexGlobal()  //activeObjectIndexに１を足す(Global)
    {
        globalActiveObjectIndex = globalActiveObjectIndex + 1;

        if (globalActiveObjectIndex >= targetObject.Length)
        {
            globalActiveObjectIndex = 0;
        }
    }

    private void SwitchActiveObjectLocal()    //activeObjectIndexをセットして反映させる
    {
        for (int x = 0; x < targetObject.Length; x = x + 1)
        {
            if (targetObject[x] != null)    //配列内のNullチェック
            {
                targetObject[x].SetActive(x == localActiveObjectIndex);      //番号に対応したオブジェクトをオンにする
            }
        }
    }

    private void SwitchActiveObjectGlobal()    //activeObjectIndexをセットして反映させる
    {
        for (int x = 0; x < targetObject.Length; x = x + 1)
        {
            if (targetObject[x] != null)    //配列内のNullチェック
            {
                targetObject[x].SetActive(x == globalActiveObjectIndex);      //番号に対応したオブジェクトをオンにする
            }
        }
    }

    private void NextObjectIndexLocal()   //次のObjectIndexに切り替える(Local)
    {
        AddActiveObjectIndexLocal();     //activeObjectIndexに１を足す
        SwitchActiveObjectLocal();       //activeObjectIndexをセットして反映させる(Local)
    }


    private void NextObjectIndexGlobal()   //次のObjectIndexに切り替える(Global)
    {
        AddActiveObjectIndexGlobal();     //activeObjectIndexに１を足す
        SwitchActiveObjectGlobal();       //activeObjectIndexをセットして反映させる(Global)
        RequestSerialization();
    }
}
