using UnityEngine;
public class SimpleBGMManager : MonoBehaviour
{
    private IMusicPlayer[] _players;
    
    private int current = -1;

    private void Awake()
    {
        _players = GetComponentsInChildren<IMusicPlayer>();
    }

    public int PlayAt(BGMId bgmId)
    {
        return PlayAt((int)bgmId);
    }
    
    public int PlayAt(int index)
    {
        _ = _players[index].Play();
        current = index;
        return current;
    }
    
    public void ResumeAt(int index)
    {
        _players[index].Resume();
        current = index;
    }

    public void StopAt(int index)
    {
        _players[index].Stop();
    }
    
    public int StopCurrent()
    {
        if (current == -1) return -1;

        _players[current].Stop();
        return current;
    }

    public void StopAll()
    {
        foreach (var player in _players)
        {
            player.Stop();
        }
    }
}

public enum BGMId : int
{
    MenuA = 0,
    MenuB,
    MenuC,
    MenuD,
    BattleA,
    BattleB,
    Clear,
}
