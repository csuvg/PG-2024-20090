using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinding : MonoBehaviour
{
    public static AStarPathfinding Instance; // Instancia singleton

    private Node[,] grid;
    private int gridSizeX, gridSizeY;
    public Vector2 gridWorldSize; // Define el tama�o del grid
    public float nodeRadius; // Radio de cada nodo
    private float nodeDiameter;

    // Awake se llama cuando el script es cargado
    private void Awake()
    {
        // asegura que solo haya una instancia de este script
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destruiye el objeto si ya existe una instancia
        }
    }

    // Start se llama antes de la primera actualizaci�n del frame
    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridWorldSize = new Vector2(20, 20); // ajustar seg�n el tama�o del grid
        CreateGrid();
    }

    // Funcion para crear el la cuadricula
    public void CreateGrid()
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // ajustar segun el tamano de la cuadricula
                Vector3 worldPoint = new Vector3(x * nodeDiameter, 0, y * nodeDiameter); 
                bool walkable = true; 
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // Funci�n para obtener el nodo en el mundo
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // determina la posici�n del nodo en el grid
        float percentX = (worldPosition.x + (gridWorldSize.x / 2)) / gridWorldSize.x;
        float percentY = (worldPosition.z + (gridWorldSize.y / 2)) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y]; // asegura que el nodo est� dentro del grid
    }

    // Funci�n para encontrar el camino entre dos puntos
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);

        Debug.Log("Start Node Walkable: " + startNode.walkable);
        Debug.Log("Target Node Walkable: " + targetNode.walkable);

        Debug.Log("Start Node Position: " + startNode.position);
        Debug.Log("Target Node Position: " + targetNode.position);

        if (startNode == null || targetNode == null)
        {
            Debug.LogError("Start or target node is null!");
            return null; // salida temprana si alguno de los nodos es nulo
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // no se encontr� un camino
    }

    // Funci�n para obtener la distancia entre dos nodos
    public int GetDistance(Node a, Node b)
    {
        if (a == null || b == null)
        {
            Debug.LogError("One of the nodes is null.");
            return 0; // retorna 0 si alguno de los nodos es nulo
        }
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        return distX + distY; // distancia Manhattan
    }

    // Funci�n para rehacer el camino
    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    // Funci�n para obtener los vecinos de un nodo
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // salta el nodo actual
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // revisa si el nodo est� dentro del grid
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

}

// Clase para representar un nodo en el grid
public class Node
{
    public bool walkable;
    public Vector3 position;
    public int gridX;
    public int gridY;
    public Node parent;
    public int gCost; // costo de movimiento desde el nodo inicial
    public int hCost; // heur�stica de costo de movimiento al nodo final
    public int fCost { get { return gCost + hCost; } } // costo total

    // Constructor de la clase
    public Node(bool _walkable, Vector3 _position, int _gridX, int _gridY)
    {
        walkable = _walkable;
        position = _position;
        gridX = _gridX;
        gridY = _gridY;
    }
}
