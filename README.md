# Open Hackathon SmartContract

The open hackathon contract helps to create, set prizes for the winners with better transparency and less transaction fee.

**User Roles**

1. Owner 
2. Hackathon Manager
3. Registered Member

**Requirements:**

- Visual Studio 2019
- SmartContract Template

## Methods
### Create Hackathon

The Owner user can create a new Hackathon using this method. It sets Hackathon details in the contract state. It will return a new Id.
It also transfers CRS token equivalent to the prize amount to the contract's address. Later when the manager announces the winner, it will be transferred
to the winner's wallet.

```
public uint CreateHackathon(string title, uint duration, Address hackathonManager)
```

### Register

Using this method, users can register with active hackathons.

```
public bool Register(uint hackathonId)
```

### Set Winner Prize 

Using this method a Hackathon Manager can set prizes for winners.

```
public bool SetWinnerPrize(uint hackathonId, uint rank, ulong amount)
```

### Announce Winner

Using this method a Hackathon Manager can announce winners. The winner will get the prize amount from the deposited CRS token in the contract.

```
public bool AnnounceWinner(uint hackathonId, uint rank, Address winnerAddress)
```