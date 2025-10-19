using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Define;

[CreateAssetMenu(fileName = "Level Data", menuName = "Scriptable Object/Level Data", order = int.MaxValue)]
public class LevelData : ScriptableObject
{
    [Serializable]
    public class Row
    {
        public List<TileType> tiles;
    }

    [SerializeField] public List<Row> rows;
    [SerializeField] public int maxCount;

    //TODO : 컬럼수 체크해서 에러 나오도록하는게 좋을 듯 n * m 으로 데이터가 만들어지도록

    public (int, int, Dictionary<(int, int), TileType>) GetData()
    {
        Dictionary<(int, int), TileType> boardData = new Dictionary<(int, int), TileType>();

        for (var i = 0; i < rows.Count; i++)
        {
            for (var j = 0; j < rows[i].tiles.Count; j++)
            {
                boardData.Add((j, -i), rows[i].tiles[j]); // y 좌표 반대로 뒤집어 준다.
            }
        }

        return (rows[0].tiles.Count, rows.Count, boardData);
    }
}