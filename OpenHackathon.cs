using Stratis.SmartContracts;
using System;

/// <summary>
/// Open Hackathon smart contract 
/// </summary>

[Deploy]
public class OpenHackathon : SmartContract
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="smartContractState">SmartContract State</param>
    /// <param name="minDuration">Min duration in blocks</param>
    public OpenHackathon(ISmartContractState smartContractState, uint minDuration)
    : base(smartContractState)
    {
        Owner = Message.Sender;
        MinDuration = minDuration;
        LastHackathonId = 1;
    }

    #region Properties
    public Address Owner
    {
        get => State.GetAddress(nameof(Owner));
        private set => State.SetAddress(nameof(Owner), value);
    }
    public uint MinDuration
    {
        get => State.GetUInt32(nameof(MinDuration));
        private set => State.SetUInt32(nameof(MinDuration), value);
    }
    public uint LastHackathonId
    {
        get => State.GetUInt32(nameof(LastHackathonId));
        private set => State.SetUInt32(nameof(LastHackathonId), value);
    }
    private void SetHackathonStruct(uint hackathoId, Hackathon hackathon) => State.SetStruct($"Hackathon:{hackathoId}", hackathon);

    public Hackathon GetHackathonStruct(uint hackathoId) => State.GetStruct<Hackathon>($"Hackathon:{hackathoId}");

    private void SetWinnerPrizeAmount(uint hackathoId, uint rank, ulong prizeAmount) => State.SetUInt64($"Hackathon:{hackathoId}:Winner:{rank}:PrizeAmount", prizeAmount);

    private ulong GetWinnerPrizeAmount(uint hackathoId, uint rank) => State.GetUInt64($"Hackathon:{hackathoId}:Winner:{rank}:PrizeAmount");

    private void SetParticipant(uint hackathoId, Address address) => State.SetBool($"Hackathon:{hackathoId}:Participant:{address}", true);

    private bool GetParticipant(uint hackathoId, Address address) => State.GetBool($"Hackathon:{hackathoId}:Participant:{address}");

    private void SetDeadline(uint hackathoId, ulong block) => State.SetUInt64($"Deadline:{hackathoId}", block);

    public ulong GetDeadline(uint hackathonId) => State.GetUInt64($"Deadline:{hackathonId}");

    #endregion

    #region Methods
    private void EnsureOwnerOnly() => Assert(this.Owner == Message.Sender, "The method is owner only.");

    private void EnsureNotPayable() => Assert(Message.Value == 0, "The method is not payable.");

    /// <summary>
    /// Create Hackathon
    /// </summary>
    /// <param name="title">Hackathon Title</param>
    /// <param name="duration">Duration Blocks</param>
    /// <param name="hackathonManager">Manager Wallet Address</param>
    /// <returns></returns>
    public uint CreateHackathon(string title, uint duration, Address hackathonManager)
    {
        EnsureOwnerOnly();        

        Assert(duration >= MinDuration, "Hackathon duration should be between higher than min duration.");

        var length = title?.Length ?? 0;
        Assert(length <= 200, "The title length can be up to 200 characters.");

        var hackathonId = LastHackathonId;

        var hackathon = new Hackathon
        {
            Id = hackathonId,
            Title = title,
            PrizeAmount = Message.Value,
            HackathonManager = hackathonManager
        };

        SetHackathonStruct(hackathonId, hackathon);

        SetDeadline(hackathonId, checked(duration + Block.Number));

        var transferResult = Transfer(this.Address, Message.Value);
        Assert(transferResult.Success, "Transfer failed.");        

        Log(hackathon);

        LastHackathonId = hackathonId + 1;

        return hackathonId;
    }

    /// <summary>
    /// Set Winner Prize
    /// </summary>
    /// <param name="hackathonId">Hackathon Id</param>
    /// <param name="rank">Winner Rank</param>
    /// <param name="amount">Amount</param>
    /// <returns></returns>
    public bool SetWinnerPrize(uint hackathonId, uint rank, ulong amount)
    {
        EnsureNotPayable();

        var hackathon = GetHackathonStruct(hackathonId);

        Assert(hackathon.Id != 0, "Hackathon with provided index is not found.");

        Assert(Message.Sender == hackathon.HackathonManager, "Only Hackathon manager can set prizes.");

        SetWinnerPrizeAmount(hackathonId, rank, amount);

        return true;
    }

    /// <summary>
    /// Register for hackathon
    /// </summary>
    /// <param name="hackathonId"></param>
    /// <returns></returns>
    public bool Register(uint hackathonId)
    {
        Assert(GetDeadline(hackathonId) > Block.Number, "Hackathon is completed.");

        var isRegistered = GetParticipant(hackathonId, Message.Sender);

        Assert(!isRegistered, "Already Registered.");

        SetParticipant(hackathonId, Message.Sender);

        Log(new ParticipantLog { HackathonId = hackathonId, Address = Message.Sender });

        return true;
    }

    /// <summary>
    /// Announce Winner
    /// </summary>
    /// <param name="hackathonId">Hackathon Id</param>
    /// <param name="rank">Winner Rank</param>
    /// <param name="winnerAddress">Winner Wallet Address</param>
    /// <returns></returns>
    public bool AnnounceWinner(uint hackathonId, uint rank, Address winnerAddress)
    {
        var hackathon = GetHackathonStruct(hackathonId);

        Assert(Message.Sender == hackathon.HackathonManager, "Only Hackathon manager can set winners.");

        Assert(GetParticipant(hackathonId, winnerAddress), "Winner address is not valid Participant.");

        var prizeAmount = GetWinnerPrizeAmount(hackathonId, rank);

        Assert(prizeAmount <= Balance, "Insufficient balance.");

        var result = Transfer(winnerAddress, prizeAmount);

        Assert(result.Success, "Transfer failed.");

        Log(new WinnerPrizeLog { Address = winnerAddress, Rank = rank, Amount = prizeAmount });

        return true;
    }

    public override void Receive() { }

    #endregion

    public struct Hackathon
    {
        [Index]
        public uint Id;

        public string Title;

        public ulong PrizeAmount;

        public Address HackathonManager;
    }

    public struct ParticipantLog
    {
        public uint HackathonId;

        public Address Address;
    }

    public struct WinnerPrizeLog
    {
        public Address Address;

        public uint Rank;

        public ulong Amount;
    }

    public struct DepositLog
    {
        [Index]
        public Address Sender;
        public ulong Amount;
    }
}