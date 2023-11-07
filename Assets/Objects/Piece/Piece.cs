using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private GameObject cubeGO;

    List<Cube> cubes = new List<Cube>();
    public List<Cube> Cubes { get => cubes; }

    public void SpawnCubes()
    {
        foreach (var cube in cubes)
        {
            Instantiate(cubeGO, transform.position + cube.pieceLocalPosition, transform.rotation, transform);
            cube.gridPosition = Grid3DManager.WorldToGridPosition(transform.position) + cube.pieceLocalPosition;
        }
    }

    public void ChangeCubes(List<Cube> _cubes)
    {
        if(_cubes.Count < 0) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        cubes = _cubes;
    }


    public List<Cube> Rotate(bool rotateLeft)
    {
        Quaternion rotation;
        if (rotateLeft)
        {
            rotation = Quaternion.Euler(0, -90, 0);
        }
        else
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }

        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        List<Cube> rotatedBlocks = new List<Cube>();

        foreach (Cube block in cubes)
        {
            Cube newBlock = new Cube();
            newBlock.pieceLocalPosition = m.MultiplyPoint3x4(block.pieceLocalPosition);
            newBlock.pieceLocalPosition = new Vector3(MathF.Round(newBlock.pieceLocalPosition.x), MathF.Round(newBlock.pieceLocalPosition.y),
                MathF.Round(newBlock.pieceLocalPosition.z));
            rotatedBlocks.Add(newBlock);
        }

        ChangeCubes(rotatedBlocks);
        return rotatedBlocks;
    }

}

[Serializable]
public class Cube
{
    [HideInInspector] public Vector3 gridPosition;
    public Vector3 pieceLocalPosition;
}
