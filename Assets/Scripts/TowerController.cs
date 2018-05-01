using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerController : MonoBehaviour
{
    private int piecesCount;
    public Button button;
    public Slider slider;
    public InputField inputField;
    public float speed = 10f;

    List<Track> tracks = new List<Track>();
    List<GameObject> pieces = new List<GameObject>();
    List<GameObject> sticks = new List<GameObject>();
    List<Vector3> endPosition = new List<Vector3>() { new Vector3(-2, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 0, 0) };

    private bool start = true;

    private void Start()
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.transform.position = new Vector3(0, -0.5f, 0);
        table.transform.localScale = new Vector3(6, 1, 3);
    }

    public void ChangeSpeed()
    {
        speed = slider.value;
    }

    public void OnClick()
    {
        if (start)
        {
            button.enabled = false;
            StartCoroutine(Init());
            button.GetComponentInChildren<Text>().text = "Остановить";            
        }
        else
        {
            button.enabled = false;
            StartCoroutine(Stop());
            button.GetComponentInChildren<Text>().text = "Решить";
        }
        start = !start;
    }

    private IEnumerator Stop()
    {
        StopCoroutine(MovePieces());
        for (int i = 0; i < pieces.Count; i++)
        {
            Destroy(pieces[i]);
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < sticks.Count; i++)
        {
            Destroy(sticks[i]);
            yield return new WaitForSeconds(0.1f);
        }

        tracks.Clear();
        pieces.Clear();
        sticks.Clear();
        button.enabled = true;
    }

    public IEnumerator Init()
    {
        piecesCount = int.Parse(inputField.text);

        for (int i = 0; i < 3; i++)
        {
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.transform.localScale = new Vector3(0.1f, 0.2f + 0.2f * piecesCount / 2, 0.1f);
            stick.transform.position = endPosition[i] + new Vector3(0, 0.2f + 0.2f * piecesCount/2, 0);
            sticks.Add(stick);

            yield return new WaitForSeconds(0.1f);
        }

        float delta = (1f - 0.5f) / piecesCount;
        for (int i = 0; i < piecesCount; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.position = new Vector3(-2, 0.1f + 0.2f * i, 0);
            obj.transform.localScale = new Vector3(1 - delta * i, 0.1f, 1 - delta * i);
            obj.GetComponent<Renderer>().material.color = Random.ColorHSV();
            pieces.Add(obj);

            yield return new WaitForSeconds(0.1f);
        }
        SolveTowerOfHanoi(piecesCount);

        pieces.Reverse();
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MovePieces());
        button.enabled = true;
    }

    void SolveTowerOfHanoi(int pieces)
    {
        SolveTowerOfHanoi(pieces, 1, 3);
    }

    void SolveTowerOfHanoi(int pieces, int startStack, int endStack)
    {
        if (pieces == 0) return;

        int middleStack = 6 - startStack - endStack;
        SolveTowerOfHanoi(pieces - 1, startStack, middleStack);
        tracks.Add(new Track(pieces -1, startStack - 1, endStack - 1));
        SolveTowerOfHanoi(pieces - 1, middleStack, endStack);
    }

    IEnumerator MovePieces()
    {
        int start = piecesCount;
        int middle = 0;
        int end = 0;

        foreach (var item in tracks)
        {
            float offset = 0;
            switch (item.endstack)
            {
                case 0:
                    offset = 0.1f + 0.2f * start;
                    start++;
                    break;
                case 1:
                    offset = 0.1f + 0.2f * middle;
                    middle++;
                    break;
                case 2:
                    offset = 0.1f + 0.2f * end;
                    end++;
                    break;
            }
            Vector3 firstPos = new Vector3(pieces[item.pieces].transform.position.x, 0.3f + 0.2f * piecesCount / 2, pieces[item.pieces].transform.position.z) + new Vector3(0, 0.4f + 0.2f * piecesCount / 2, 0);
            Vector3 middlePos = new Vector3(endPosition[item.endstack].x, firstPos.y, endPosition[item.endstack].z); 
            Vector3 endPos = endPosition[item.endstack] + new Vector3(0, offset, 0);

            while (pieces[item.pieces].transform.position != firstPos)
            {
                pieces[item.pieces].transform.position = Vector3.MoveTowards(pieces[item.pieces].transform.position, firstPos, Time.deltaTime * speed);
                yield return null;
            }

            while (pieces[item.pieces].transform.position != middlePos)
            {
                pieces[item.pieces].transform.position = Vector3.MoveTowards(pieces[item.pieces].transform.position, middlePos, Time.deltaTime * speed);
                yield return null;
            }

            while (pieces[item.pieces].transform.position != endPos)
            {
                pieces[item.pieces].transform.position = Vector3.MoveTowards(pieces[item.pieces].transform.position, endPos, Time.deltaTime * speed);
                yield return null;
            }
            switch (item.startStack)
            {
                case 0:
                    start--;
                    break;
                case 1:
                    middle--;
                    break;
                case 2:
                    end--;
                    break;
            }
        }
        button.GetComponentInChildren<Text>().text = "Решено";
    }
}

public class Track
{
    public int pieces;
    public int startStack;
    public int endstack;

    public Track(int pieces, int startStack, int endstack)
    {
        this.pieces = pieces;
        this.startStack = startStack;
        this.endstack = endstack;
    }
}
