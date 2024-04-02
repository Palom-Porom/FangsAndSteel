using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewUnitUIManager : MonoBehaviour
{
    public static NewUnitUIManager Instance { get; private set; }

    public Image ShootOnMoveButton { get { return shootOnMoveButton; } }
    [SerializeField] private Image shootOnMoveButton;

    public Image ShootOffButton { get { return shootOffButton; } }
    [SerializeField] private Image shootOffButton;

    public Image AutoPursuitButton { get { return autoPursuitButton; } }
    [SerializeField] private Image autoPursuitButton;

    public Image EnemyListBut { get { return enemyListBut; } }
    [SerializeField] private Image enemyListBut;

    public Image PursuiteInfantryBut { get { return pursuiteInfantryBut; } }
    [SerializeField] private Image pursuiteInfantryBut;
    public Image PursuiteMachineGunnerBut { get { return pursuiteMachineGunnerBut; } }
    [SerializeField] private Image pursuiteMachineGunnerBut;
    public Image PursuiteAntyTankBut { get { return pursuiteAntyTankBut; } }
    [SerializeField] private Image pursuiteAntyTankBut;
    public Image PursuiteTankBut { get { return pursuiteTankBut; } }
    [SerializeField] private Image pursuiteTankBut;
    public Image PursuiteArtilleryBut { get { return pursuiteArtilleryBut; } }
    [SerializeField] private Image pursuiteArtilleryBut;

    #region Sliders

    public Slider PursuiteStartRadiusSlider { get { return pursuiteStartRadiusSlider; } }
    [SerializeField] private Slider pursuiteStartRadiusSlider;
    public TextMeshProUGUI PursuiteStartRadiusSliderText { get { return pursuiteStartRadiusSliderText; } }
    [SerializeField] private TextMeshProUGUI pursuiteStartRadiusSliderText;

    public Slider PursuiteMaxHpSlider { get { return pursuiteMaxHpSlider; } }
    [SerializeField] private Slider pursuiteMaxHpSlider;
    public TextMeshProUGUI PursuiteMaxHpSliderText { get { return pursuiteMaxHpSliderText; } }
    [SerializeField] private TextMeshProUGUI pursuiteMaxHpSliderText;

    public Slider PursuiteEndRadiusSlider { get { return pursuiteEndRadiusSlider; } }
    [SerializeField] private Slider pursuiteEndRadiusSlider;
    public TextMeshProUGUI PursuiteEndRadiusSliderText { get { return pursuiteEndRadiusSliderText; } }
    [SerializeField] private TextMeshProUGUI pursuiteEndRadiusSliderText;

    public Slider PursuiteMaxAttackDistSlider { get { return pursuiteMaxAttackDistSlider; } }
    [SerializeField] private Slider pursuiteMaxAttackDistSlider;
    public TextMeshProUGUI PursuiteMaxAttackDistSliderText { get { return pursuiteMaxAttackDistSliderText; } }
    [SerializeField] private TextMeshProUGUI pursuiteMaxAttackDistSliderText;

    #endregion

    void Awake()
    {
        Instance = this;
    }
}
