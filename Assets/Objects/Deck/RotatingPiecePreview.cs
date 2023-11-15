using HelperScripts.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPiecePreview : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private EventObjectScriptable onPiecePlacedPiece;

    [Header("Piece")]
    [SerializeField] Piece piecePrefab;
    Piece piece;

    [Header("Rotation")]
    [SerializeField] Vector3 rotationVector;
    [SerializeField] float rotationSpeed;

    private void Awake()
    {
        
    }

    void Start()
    {
        //Grid3DManager.Instance.OnCubeChange += OnPieceChange;
        onPiecePlacedPiece.AddListener(OnPieceChange);

        piece = Instantiate(piecePrefab, transform);
        piece.ChangePiece(Grid3DManager.Instance.pieceSo);
        piece.SpawnCubes();
    }

    private void OnPieceChange(object _newPiece)
    {
        transform.rotation = new Quaternion(0,0,0,0);
        PieceSO newPiece = (PieceSO)_newPiece;
        piece.ChangePiece(newPiece);
        piece.SpawnCubes();
    }

    private void Update()
    {
        transform.Rotate(rotationVector, rotationSpeed, Space.World);
    }

}
