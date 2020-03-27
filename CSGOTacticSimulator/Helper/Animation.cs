using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticSimulator.Helper
{
    public enum Status
    {
        Waiting,
        Running,
        Finished
    }

    public enum Action
    {
        WaitFor,
        WaitUntil,
        Run,
        Walk,
        Squat,
        Teleport,
        Shoot,
        ChangeStatus,
        ChangeVerticalPosition,
        Throw,
        Plant,
        Defuse
    }
    public enum ActionLimit
    {
        RunJumpOnly,
        RunOrWalkJump,
        Jump,
        RunOnly,
        WalkOnly,
        SquatOnly,
        RunOrWalk,
        WalkOrSquat,
        RunClimbOrFall,
        WalkClimb,
        AllAllowed
    }
    public enum VolumeLimit
    {
        Quietly,
        Noisily
    }
    public class Animation
    {
        public int ownerIndex;

        public Status status = Status.Waiting;
        public Point endMapPoint;
        public Action action;
        public object[] objectPara;
        public Animation(int ownerIndex, Action action, Point endMapPoint, params object[] obj)
        {
            this.ownerIndex = ownerIndex;
            this.action = action;
            this.endMapPoint = endMapPoint;
            this.objectPara = obj;
        }
    }
}
