using System;
using System.Collections.Generic;
using UnityEngine;

public class RobotCommand
{
    public Vector2Int MinPos { get; protected set; }
    public Vector2Int MaxPos { get; protected set; }

    public Vector2Int StartPos { get; protected set; }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public class Step
    {
        public Direction Direction;
        public int Distance;

        public override string ToString()
        {
            return $"{Direction}({Distance})";
        }
    }
    public List<Step> Steps {  get; protected set; }

    // M:-10,10,-10,10;S:-5,5;[W5,E5,N4,E3,S2,W1]

    // TODO: All Parse calls should probably be replaced by TryParse and checked for validity.
    public RobotCommand(string command)
    {
        string[] sections = command.Split(';');
        if (sections.Length != 3)
        {
            Debug.LogError($"Invalid command format. Expected three sections but got {sections.Length}");
            return;
        }

        foreach (var section in sections)
        {
            if (section.StartsWith("M:"))
            {
                string[] map = section.Substring(2).Split(",");
                if (map.Length != 4)
                {
                    Debug.LogError("Invalid map format.");
                    return;
                }
                MinPos = new Vector2Int(int.Parse(map[0]), int.Parse(map[2]));
                MaxPos = new Vector2Int(int.Parse(map[1]), int.Parse(map[3]));
                continue;
            }
            if (section.StartsWith("S:"))
            {
                string[] start = section.Substring(2).Split(",");
                if (start.Length != 2)
                {
                    Debug.LogError("Invalid start format.");
                    return;
                }
                StartPos = new Vector2Int(int.Parse(start[0]), int.Parse(start[1]));
                continue;
            }
            if (section.StartsWith("[") && section.EndsWith("]"))
            {
                Steps = new List<Step>();
                foreach(string s in section.Substring(1, section.Length - 2).Split(","))
                {
                    Step step = new Step();
                    try
                    {
                        step.Direction = s[0] switch
                        {
                            'N' => Direction.North,
                            'E' => Direction.East,
                            'S' => Direction.South,
                            'W' => Direction.West,
                            _ => throw new ArgumentOutOfRangeException(nameof(Direction), "Unexpected direction value")
                        };
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Debug.LogError ($"Invalid step format.");
                        return;
                    }

                    step.Distance = int.Parse(s.Substring(1));
                    Steps.Add(step);
                } 
                continue;
            }
            Debug.LogError($"Invalid command format. Unknown section {section}");
            return;
        }

    }

    /// <summary>
    /// Returns the next direction to move in or null if we reached the target.
    /// </summary>
    /// <returns></returns>
    public Direction? GetNextDirection()
    {
        if (Steps.Count == 0)
        {
            return null; // We are done.
        }

        var dir = Steps[0].Direction;
        Steps[0].Distance--;
        if (Steps[0].Distance <= 0)
        {
            Steps.RemoveAt(0);
        }

        return dir;
    }

    public override string ToString()
    {
        return $"MinPos={MinPos}, MaxPos={MaxPos}, StartPos={StartPos}, Steps={string.Join<Step>(", ", Steps)}";
    }

}
public class RobotController : MonoBehaviour
{
    public float Speed = 2f;
    public float RotateDamping = 0.01f;

    private RobotCommand _command;
    private Vector3 _velocity;
    private Vector3 _target;


    public void Activate(RobotCommand command)
    {
        Debug.Log(command);
        _command = command;

        _target = new Vector3(_command.StartPos.x, 0f, _command.StartPos.y);
    }

    public void Activate(string command)
    {
        Activate(new RobotCommand(command));
    }

    private void Update()
    {
        if (_command == null)
        {
            return;
        }

        if ((_target - transform.position).sqrMagnitude < 0.01f)
        {
            transform.position = _target;

            RobotCommand.Direction? dir = _command.GetNextDirection();
            if (dir == null)
            {
                Debug.Log("Target reached");
                _command = null;
                return;
            }

            Vector3 offset = dir switch
            {
                RobotCommand.Direction.North => new Vector3(0f, 0f, 1f),
                RobotCommand.Direction.East => new Vector3(1f, 0f, 0f),
                RobotCommand.Direction.South => new Vector3(0f, 0f, -1f),
                RobotCommand.Direction.West => new Vector3(-1f, 0f, 0f),
                _ => throw new NotImplementedException(),
            };
            // Boundary check
            if (   _target.x < _command.MinPos.x || _target.x >= _command.MaxPos.x
                || _target.y < _command.MinPos.y || _target.y >= _command.MaxPos.y)
            {
                _target = offset = Vector3.zero;
                _command = null;
                Debug.Log($"Unable to reach target. Target is outside the map. ({Mathf.FloorToInt(transform.position.x)}, {Mathf.FloorToInt(transform.position.y)})");
            }
            _target = transform.position + offset;

            // TODO: Track path
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, _target, ref _velocity, 1f / Speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_target - transform.position), RotateDamping);
    }
}
