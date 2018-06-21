namespace War.Game
{
    public enum PostureState
    {
        Stand = 0,
        Crouch = 1,
        Prone = 2,
        Wound = 3,
        Drive = 4,
        Ride = 5,
        Parachute = 6,
        Swim = 7,
        Die = 8,
        Leave = 9,
    }

    public enum AimingState
    {
        Invalid = -1,
        Stand = 0,
        Aim,
        ADS
    }

    public enum MoveState
    {
        Stand = 0,
        Walk,
        Run,
        Jump
    }

    public enum BodyPart
    {
        Head = 0,
        Body = 1,
        Hand = 2,
        Leg  =3,
    }

    public enum HitActionType
    {
        Character = 0,
        Tank = 1,
    }

    public enum WeaponType
    {
        Unarmed = 0,
        Gun = 1,
        Missile = 2,
        Melee = 3,
    }

    public enum CharacterControlState
    {
        Stand = 0,
        Crouch = 1,
        Prone = 2,
        Wound = 3,
        Drive = 4,
        Ride = 5,
        Parachute = 6,
        Swin = 7,
        Die = 8,
        Leave = 9,
    }
}