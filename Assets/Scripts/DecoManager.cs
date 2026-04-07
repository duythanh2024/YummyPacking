
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class DecoManager : MonoBehaviour
{
    public Button Btn_Play, Btn_Arena;
    public GameObject Groups;
    public Image Img_Floor;
    public Image Img_grass;
    public Image Img_Wall;
    public Image Img_Roof;
    public Image Img_Decor;
    public Image Img_Table;
    public Image Img_Balcony;
    public Image Img_eaves;
    public Image Img_tools;
    public Image Img_light;
    [Header("Floors")]
    public Sprite[] floors;

    [Header("Grass")]
    public Sprite[] grasses;

    [Header("Wall")]
    public Sprite[] walls;

    [Header("Roof")]
    public Sprite[] roofs;

    [Header("Decor")]
    public Sprite[] decors;

    [Header("Table")]
    public Sprite[] tables;

    [Header("Balcony")]
    public Sprite[] balconies;

    [Header("Eaves")]
    public Sprite[] eaves;

    [Header("Tools")]
    public Sprite[] tools;

    [Header("Light")]
    public Sprite[] lights;



    public DecoDatabase database;
    public GameObject itemArenPrefab;
    public GameObject pnl_Arena;
    public Transform contentTransform;
    public GameObject pnl_Option_Arena;
    public ParticleSystem particleWin;
    public TextMeshProUGUI Txt_Stars;
    public TextMeshProUGUI Txt_Mission;
    private Vector3[] positionLTools = new Vector3[4];
    private Vector3[] positionLGrass = new Vector3[4];
    private Vector3[] positionTables = new Vector3[4];

    void Start()
    {
        positionLTools[0] = new Vector3(4, 89, 0);
        positionLTools[1] = new Vector3(13, 109, 0);
        positionLTools[2] = new Vector3(42, 97, 0);
        positionLTools[3] = new Vector3(6, 80, 0);

        positionLGrass[0] = new Vector3(-11, 73, 0);
        positionLGrass[1] = new Vector3(-17, 73.5f, 0);
        positionLGrass[2] = new Vector3(61, 80, 0);
        positionLGrass[3] = new Vector3(-11, 62f, 0);


        positionTables[0] = new Vector3(32, 73, 0);
        positionTables[1] = new Vector3(32, 62, 0);
        positionTables[2] = new Vector3(-40, 55, 0);
        positionTables[3] = new Vector3(57, 50, 0);

        GetMission();
        LoadHome();
    }

    public void ShowButton(bool display)
    {
        AudioManager.Instance.Play("Click");
        Btn_Play.gameObject.SetActive(display);
        Btn_Arena.gameObject.SetActive(display);
        Groups.SetActive(display);
    }
    public void ShowOption_Arena(DecoItem decoItem)
    {
        pnl_Option_Arena.SetActive(true);

        Option_Arena option_Arena = pnl_Option_Arena.GetComponent<Option_Arena>();
        option_Arena.SetData(decoItem, this);
    }
    public void ShowArena(bool display = true)
    {
        AudioManager.Instance.Play("Click");
        // int   levelNo = PlayerPrefs.GetInt("Level", 1);
        // if (levelNo < 10)
        // {
        //     ToastManager.Instance.ShowToast("Unlock level 10");
        //     return;
        // }
        ShowButton(false);
        pnl_Arena.SetActive(display);
        if (display)
        {
            LoadData();
        }
    }

    public List<DecoItem> GetallItems()
    {
        return database.allItems;
    }

    public void LoadData()
    {
        ClearContent(contentTransform);
        List<DecoItem> allItems = database.allItems;
        foreach (DecoItem item in allItems)
        {
            GameObject itemArenaPre = Instantiate(itemArenPrefab, contentTransform);
            ItemArena itemArena = itemArenaPre.GetComponent<ItemArena>();
            itemArena.SetDecoManager(this);
            itemArena.ShowData(item);
        }
    }
    public void GetMission()
    {
        int countActive = 0;
        List<DecoItem> allItems = database.allItems;
        for (int i = 0; i < allItems.Count; i++)
        {
            DecoItem item = allItems[i];
            bool isFinish = PlayerPrefs.GetInt("items" + item.id, 0) == 1;
            item.isFinish = isFinish;
            if (isFinish)
            {
                countActive++;
            }
        }
        Txt_Mission.text = (countActive + "/" + allItems.Count);
    }

    public void ClearContent(Transform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(contentTransform.GetChild(i).gameObject);
        }
    }
    void LoadHome()
    {
        int defaultValues = 0;
        int floor = PlayerPrefs.GetInt("Floor", defaultValues);
        int grass = PlayerPrefs.GetInt("Grass", defaultValues);
        int wall = PlayerPrefs.GetInt("Wall", defaultValues);
        int roof = PlayerPrefs.GetInt("Roof", defaultValues);
        int decor = PlayerPrefs.GetInt("Decor", defaultValues);
        int table = PlayerPrefs.GetInt("Table", defaultValues);
        int balcony = PlayerPrefs.GetInt("Balcony", defaultValues);//
        int eave = PlayerPrefs.GetInt("Eaves", defaultValues); //
        int tool = PlayerPrefs.GetInt("Tools", defaultValues);
        int light = PlayerPrefs.GetInt("Light", defaultValues);

        Img_Floor.sprite = floors[floor];
        Img_grass.sprite = grasses[grass];
        Img_grass.GetComponent<RectTransform>().localPosition = positionLGrass[grass];
        Img_Wall.sprite = walls[wall];
        Img_Roof.sprite = roofs[roof];
        Img_Decor.sprite = decors[decor];
        Img_Table.sprite = tables[table];
        Img_Table.GetComponent<RectTransform>().localPosition = positionTables[table];
        if (balcony == 0)
        {
            Img_Balcony.gameObject.SetActive(false);
        }
        else
        {
            Img_Balcony.sprite = balconies[balcony];
            Img_Balcony.gameObject.SetActive(true);
        }

        if (eave == 0)
        {
            Img_eaves.gameObject.SetActive(false);
        }
        else
        {
            Img_eaves.sprite = eaves[eave];
            Img_eaves.gameObject.SetActive(true);
        }
        Img_tools.sprite = tools[tool];
        Img_tools.GetComponent<RectTransform>().localPosition = positionLTools[tool];
        Img_light.sprite = lights[light];
    }

    public void Build(int decoItemId, int optionId)
    {
        AudioManager.Instance.Play("Click");
        this.optionId = 0;
        this.decoItemId = 0;
        this.decoItemId = decoItemId;
        this.optionId = optionId;
        if (decoItemId == 7) //Wall
        {
            optionId++;
            Img_Wall.sprite = walls[optionId];
        }
        else if (decoItemId == 3)
        {
            optionId++;
            Img_Roof.sprite = roofs[optionId];
        }
        else if (decoItemId == 9)
        {
            optionId++;
            Img_Floor.sprite = floors[optionId];
        }
        else if (decoItemId == 1)
        {
            optionId++;
            Img_Decor.sprite = decors[optionId];
        }
        else if (decoItemId == 2)
        {
            optionId++;
            Img_Table.sprite = tables[optionId];
            Img_Table.GetComponent<RectTransform>().localPosition = positionTables[optionId];
        }
        else if (decoItemId == 4)
        {
            optionId++;
            Img_Balcony.gameObject.SetActive(true);

            Img_Balcony.sprite = balconies[optionId];
            if (optionId > 0)
            {
                Img_Balcony.gameObject.SetActive(true);
            }

        }
        else if (decoItemId == 6)
        {
            optionId++;
            Img_eaves.gameObject.SetActive(false);

            Img_eaves.sprite = eaves[optionId];
            if (optionId > 0)
            {
                Img_eaves.gameObject.SetActive(true);
            }

        }
        else if (decoItemId == 5)
        {
            optionId++;
            Img_tools.sprite = tools[optionId];
            Img_tools.GetComponent<RectTransform>().localPosition = positionLTools[optionId];
        }
        else if (decoItemId == 8)
        {
            optionId++;
            Img_grass.sprite = grasses[optionId];
            Img_grass.GetComponent<RectTransform>().localPosition = positionLGrass[optionId];
        }
        else if (decoItemId == 10)
        {
            optionId++;
            Img_light.sprite = lights[optionId];
        }

    }
    private int decoItemId = 0;
    private int optionId = 0;

    public void ConfirmBuild()
    {
        AudioManager.Instance.Play("Click");
        if (decoItemId == 7) //Wall
        {
            this.optionId++;

            PlayerPrefs.SetInt("Wall", this.optionId);
            PlayerPrefs.Save();


        }
        else if (decoItemId == 3)
        {
            this.optionId++;
            Img_Roof.sprite = roofs[optionId];
            PlayerPrefs.SetInt("Roof", this.optionId);
            PlayerPrefs.Save();

        }
        else if (decoItemId == 9)
        {
            this.optionId++;
            Img_Floor.sprite = floors[optionId];
            PlayerPrefs.SetInt("Floor", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 1)
        {
            this.optionId++;
            Img_Decor.sprite = decors[optionId];
            PlayerPrefs.SetInt("Decor", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 2)
        {
            this.optionId++;
            Img_Table.sprite = tables[optionId];
            Img_Table.SetNativeSize();
            PlayerPrefs.SetInt("Table", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 4)
        {
            this.optionId++;
            Img_Balcony.sprite = balconies[optionId];
            PlayerPrefs.SetInt("Balcony", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 6)
        {
            this.optionId++;
            Img_eaves.sprite = eaves[optionId];
            PlayerPrefs.SetInt("Eaves", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 5)
        {
            this.optionId++;
            Img_tools.sprite = tools[optionId];
            PlayerPrefs.SetInt("Tools", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 8)
        {
            this.optionId++;
            Img_grass.sprite = grasses[optionId];
            PlayerPrefs.SetInt("Grass", this.optionId);
            PlayerPrefs.Save();
        }
        else if (decoItemId == 10)
        {
            this.optionId++;
            Img_light.sprite = lights[optionId];
            PlayerPrefs.SetInt("Light", this.optionId);
            PlayerPrefs.Save();
        }


        if (decoItemId > 0)
        {
            PlayerPrefs.SetInt("items" + decoItemId, 1);
            LoadHome();
            //phao hoa an hung
            //an popop
            pnl_Option_Arena.SetActive(false);
            AudioManager.Instance.Play("Confetti");
            particleWin.gameObject.SetActive(true);
            particleWin.Play();
            StartCoroutine(ShowHome());
            // ShowArena();
        }

    }

    IEnumerator ShowHome()
    {
        yield return new WaitForSeconds(3.0f);
        particleWin.Stop();
        particleWin.gameObject.SetActive(false);
        GameData.Stars += -1;
        GameData.Save();
        Txt_Stars.text = GameData.Stars.ToString();
        ShowArena();
        GetMission();

    }

    public void CloseArena()
    {
        AudioManager.Instance.Play("Click");
        pnl_Option_Arena.SetActive(false);
        LoadHome();
        ShowButton(true);

    }






}
