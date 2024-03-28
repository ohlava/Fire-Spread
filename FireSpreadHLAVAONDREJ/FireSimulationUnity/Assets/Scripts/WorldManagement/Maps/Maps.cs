using System.Collections.Generic;
using System.Linq;

public class Map<T>
{
    public T[,] Data { get; set; }
    public int Width { get; }
    public int Depth { get; }

    public Map(int width, int depth)
    {
        Data = new T[width, depth];
        Width = width;
        Depth = depth;
    }

    public void FillWithDefault(T defaultValue)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Depth; j++)
            {
                Data[i, j] = defaultValue;
            }
        }
    }

    public static implicit operator Map<T>(T[,] data)
    {
        return new Map<T>(data.GetLength(0), data.GetLength(1)) { Data = data };
    }

    public static implicit operator Map<T>(T[][] data)
    {
        int width = data.Length;
        int maxDepth = data.Max(row => row.Length); // get the maximum length

        Map<T> map = new Map<T>(width, maxDepth);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < data[i].Length; j++) // only iterate to the length of the current row
                map.Data[i, j] = data[i][j];
        }

        return map;
    }

    public T[,] To2DArray()
    {
        return Data;
    }

    public T[][] ToJaggedArray()
    {
        T[][] jagged = new T[Width][];
        for (int i = 0; i < Width; i++)
        {
            jagged[i] = new T[Depth];
            for (int j = 0; j < Depth; j++)
                jagged[i][j] = Data[i, j];
        }
        return jagged;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < Depth; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                // Attempt to format if T is a float.
                if (Data[j, i] is float)
                {
                    // Explicitly format the float with two decimal places.
                    sb.Append(string.Format("{0:0.00}", Data[j, i]));
                }
                else
                {
                    sb.Append(Data[j, i]);
                }

                // Check if it's not the last element in the row to append a comma and space.
                if (j < Width - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}