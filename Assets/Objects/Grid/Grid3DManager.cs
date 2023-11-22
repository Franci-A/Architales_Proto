using HelperScripts.EventSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid3DManager : MonoBehaviour
{
    // Singleton
    private static Grid3DManager instance;
    public static Grid3DManager Instance { get => instance; }
    [SerializeField] private GameplayDataSO gameplayData;

    [Header("Grid")]
    [SerializeField] GridData data;
    public int GetHigherBlock { get => higherBlock; }
    int higherBlock = 1;

    bool isBalanceBroken;
    private Vector2 balance;

    [Header("Residents")]
    [SerializeField] private IntVariable totalNumResidents;

    [Header("Mouse Check")]
    [SerializeField] private float maxDistance = 15;
    [SerializeField] private LayerMask cubeLayer;

    [Header("Piece")]
    [SerializeField] PieceSO lobbyPiece;
    [SerializeField] private Piece piece;
    [SerializeField] ListOfBlocksSO pieceListRandom; // liste des trucs random

    [Header("Event")]
    [SerializeField] private EventScriptable onPiecePlaced;
    [SerializeField] private EventObjectScriptable onPiecePlacedPiece;
    [SerializeField] public EventScriptable onBalanceBroken;
    public delegate void OnCubeChangeDelegate(PieceSO newPiece);
    public event OnCubeChangeDelegate OnCubeChange;
    public delegate void OnLayerCubeChangeDelegate(int higherCubeValue);
    public event OnLayerCubeChangeDelegate OnLayerCubeChange;

    private List<Cube> cubeList; // current list
    private PieceSO currentPiece; // current list
    private PieceSO nextPiece;

    public PieceSO pieceSo { get => currentPiece; } // get
    public List<Cube> CubeList { get => cubeList; } // get
    private List<Cube> _cubeList; // check si ca a changer

    private TowerLeaningFeedback feedback;

    public Vector2 BalanceValue => balance * gameplayData.balanceMultiplierVariable.value;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        onPiecePlaced.AddListener(UpdateDisplacement);
    }

    private void Start()
    {
        feedback = GetComponent<TowerLeaningFeedback>();
        SpawnBase();
    }

    private void Update()
    {
        IsBlockChanged();
    }

    //INPUTS
    public void LeftClickInput(InputAction.CallbackContext context)
    {
        if (!context.performed || isBalanceBroken) return;
        TryPlacePiece();
    }

    public void RotatePieceInput(InputAction.CallbackContext context)
    {
        if (!context.performed || isBalanceBroken) return;
        RotatePiece(context.ReadValue<float>() < 0);
    }

    public void PlacePiece(Vector3 gridPos)
    {
        var piece = Instantiate(this.piece, transform);

        PieceSO pieceSO = ScriptableObject.CreateInstance<PieceSO>();
        pieceSO.cubes = CubeList;
        pieceSO.resident = currentPiece.resident;

        piece.SpawnPiece(pieceSO, gridPos);
        piece.CheckResidentLikesImpact();

        foreach (var block in piece.Cubes)
        {
            data.AddToGrid(block.gridPosition, block.cubeGO);
            UpdateWeight(block.gridPosition);

            if (block.gridPosition.y > higherBlock)
                higherBlock = (int)block.gridPosition.y;
        }

        totalNumResidents.Add(piece.Cubes.Count);

        onPiecePlaced.Call();
        OnLayerCubeChange?.Invoke(higherBlock);

        ChangePieceSORandom();
        onPiecePlacedPiece.Call(nextPiece);
    }

    private void SpawnBase()
    {
        cubeList = lobbyPiece.cubes;
        currentPiece = lobbyPiece;
        PlacePiece(Vector3.zero);
    }

    private void IsBlockChanged()
    {
        if (_cubeList != CubeList)
        {
            _cubeList = CubeList;
            PieceSO pieceSO = ScriptableObject.CreateInstance<PieceSO>();
            pieceSO.cubes = CubeList;
            pieceSO.resident = currentPiece.resident;
            OnCubeChange?.Invoke(pieceSO);
            piece.ChangePiece(pieceSO);
        }
    }

    private void RotatePiece(bool rotateLeft)
    {
        cubeList = piece.Rotate(rotateLeft);
    }

    private void TryPlacePiece()
    {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, maxDistance, cubeLayer)) return;

        Vector3 gridPos = data.WorldToGridPosition(hit.point + hit.normal / 4f);

        if (data.IsPiecePlaceValid(piece, gridPos, out Vector3 validPos))
            PlacePiece(validPos);
    }

    private void ChangePieceSORandom()
    {
        if(nextPiece == null)
        {
            nextPiece = pieceListRandom.GetRandomPiece();
        }

        currentPiece = nextPiece;
        cubeList = currentPiece.cubes;
        nextPiece = pieceListRandom.GetRandomPiece();
        
    }

    private void UpdateWeight(Vector3 gridPosistion)
    {
        balance.x += gridPosistion.x;
        balance.y += gridPosistion.z;

        if (!isBalanceBroken)
        {
            if (Mathf.Abs(BalanceValue.x) > gameplayData.MaxBalance)
            {
                isBalanceBroken = true;
                onBalanceBroken.Call();
            }

            if (Mathf.Abs(BalanceValue.y) > gameplayData.MaxBalance)
            {
                isBalanceBroken = true;
                onBalanceBroken.Call();
            }
        }
    }

    public void DestroyTower()
    {
        feedback.DestroyTower();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).origin, Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).origin + Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).direction * maxDistance);

        if (!data) return;

        for (int z = -1; z < 2; ++z)
            for (int x = -1; x < 2; ++x)
                Gizmos.DrawWireCube(data.GridToWorldPosition(new Vector3(x, 0, z)) + Vector3.down * data.CellSize * .5f, new Vector3(data.CellSize, 0, data.CellSize));

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, maxDistance, cubeLayer))
        {
            Vector3 gridPos = data.GridToWorldPosition(data.WorldToGridPosition(hit.point));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(gridPos, Vector3.one * .9f * data.CellSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(gridPos + Vector3.down * data.CellSize * .5f, new Vector3(data.CellSize, 0, data.CellSize));
        }
    }
}
