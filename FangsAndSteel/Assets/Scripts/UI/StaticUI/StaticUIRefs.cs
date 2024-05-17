using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
        
public class StaticUIRefs : MonoBehaviour
{
    public static StaticUIRefs Instance { get; private set; }
    public TextMeshProUGUI HpText { get { return hpText; } }
    private TextMeshProUGUI hpText;
    public TextMeshProUGUI AttackText { get { return attackText; } }
    private TextMeshProUGUI attackText;
    public TextMeshProUGUI ReloadText { get { return reloadText; } }
    private TextMeshProUGUI reloadText;
    public TextMeshProUGUI AttackRadiusText { get { return attackRadiusText; } }
    private TextMeshProUGUI attackRadiusText;
     public TextMeshProUGUI MovementText { get { return movementText; } }
    private TextMeshProUGUI movementText;
    public TextMeshProUGUI VisionRadiusText { get { return visionRadiusText; } }
    private TextMeshProUGUI visionRadiusText;

    [SerializeField] private GameObject unitStats;

    public Image TurnIndicator { get { return turnIndicator; } }
    [SerializeField] private Image turnIndicator;
    public TextMeshProUGUI TurnTimer { get { return turnTimer; } }
    [SerializeField] private TextMeshProUGUI turnTimer;

    public GameObject UnitsUI { get { return unitsUI; } }
    [SerializeField] private GameObject unitsUI;

    public Image ShootOnMoveButton { get { return shootOnMoveButton; } }
    [SerializeField] private Image shootOnMoveButton;

    public GameObject NewTurnPanel { get { return newTurnPanel; } }
    [SerializeField] private GameObject newTurnPanel;
    public TextMeshProUGUI NewTurnText { get { return newTurnText; } }
    [SerializeField] private TextMeshProUGUI newTurnText;

    public GameObject BuyPanel { get { return buyPanel; } }
    [SerializeField] private GameObject buyPanel;

    public TextMeshProUGUI BalanceText { get { return balanceText; } }
    [SerializeField] private TextMeshProUGUI balanceText;

    public GameObject BuyBorders { get { return buyBorders; } }
    [SerializeField] private GameObject buyBorders;

    void Awake()
    {
        Instance = this;

        newPursuitStartRadius = -1;
    }
    
    private void Start()
    {
        //hpText = unitStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        //attackText = unitStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        //reloadText = unitStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        //attackRadiusText = unitStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        //movementText = unitStats.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        //visionRadiusText = unitStats.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
    }

    [HideInInspector] public bool endTurnBut;
    [HideInInspector] public bool stopMoveBut;


    [HideInInspector] public bool shootOnMoveBut;
    [HideInInspector] public bool shootOffBut;
    [HideInInspector] public bool autoPursuitBut;
    [HideInInspector] public bool enemyListBut;
    [HideInInspector] public UnitTypes newPursuiteUnitType;
    [HideInInspector] public float newPursuitStartRadius;
    [HideInInspector] public float newPursuitmaxHp;
    [HideInInspector] public float newPursuitEndRadius;
    [HideInInspector] public float newPursuitMinAttackRadius;
    [HideInInspector] public float newPursuitTimeForEnd;

    [HideInInspector] public bool isNeededPrioritiesUpdate;


    [HideInInspector] public bool newTurnStartBut;



    [HideInInspector] public bool removeUnitButton;
    [HideInInspector] public bool buyInfantryManButton;
    [HideInInspector] public bool buyMachineGunnerButton;
    [HideInInspector] public bool buyAntiTankButton;
    [HideInInspector] public bool buyTankButton;
    [HideInInspector] public bool buyArtilleryButton;

    public void SetNewTurnButton() { newTurnStartBut = true; }
}
