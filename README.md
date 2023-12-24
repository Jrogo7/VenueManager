# Venue Manager

Simple Dalmund Plugin designed to help manage FFXIV Venues.  

The plugin provides:   

- Chat and Sound alerts for:  
   - A new player enters a house
   - A player leaves a house 
   - A player re-enters a house, including the amount of entries from that player 
   - Players are already in a house you enter 
- Guests tab that shows current status of guests in the venue
- Download json or csv reports for the guest list 
- View guest lists for saved venues you have visited
- A user created list of saved Venues 
- Ability to copy the address for the venue you are in and ones that are saved 
- Settings to customize most if not all of these features

Check out the [User Guide](https://github.com/Jrogo7/VenueManager/wiki/User-Guide) for more information. 

## Installation 

Add the following line to Dalamud Settings -> Experimental -> Custom Plugin Repositories 

```
https://raw.githubusercontent.com/Jrogo7/VenueManager/master/repo.json
```

## Commands 

`/venue` -> Open Main interface  
`/venue snooze` -> Pause alarms until you exit the current house you are in. Sending the command again will unpause  

### Aliases

`/club` -> Alias for `/venue`  
`/vm` -> Alias for `/venue`  
`/club` -> Alias for `/venue snooze`  
`/vm` -> Alias for `/venue snooze`  

