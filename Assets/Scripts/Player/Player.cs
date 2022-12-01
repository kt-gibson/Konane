using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    public abstract class Player
    {
        public event System.Action<Move> onMoveChosen;
        public event System.Action<Coord> onStartMoveChosen;

        public abstract void Update();

        public abstract void NotifyTurnToMove();

        public abstract void NotifyOpeningTurnToMove();

        //Note: I suspect Lague assigns the delegate function inside GameManger.OnMoveChosen since he sets player.onMoveChose -= OnMoveChosen
        //This would align with a delegate definition, where the delegate has to be assigned some function at some point.
        //Advantage of delegates is that they can be assigned to void functions, accessing some of the event functionality
        //General flow -> onMoveChose invokes the assigned function in game manager, which in turn updates the game state and refreshes the board UI through various function calls
        //See OnMoveChosen for reference. Can reverse engineer for my purposes
        //There's also a flag to be used that will reassign the playerToMove variable. The Game Manager update method constantly calls the player to move update method
        protected virtual void ChosenMove (Move move)
        {
            onMoveChosen?.Invoke(move); // This will use GameManager's ChosenMove function - used for regular play
        }
        
        protected virtual void ChosenStartMove(Coord move)
        {
            onStartMoveChosen?.Invoke(move); // This will use GameManager's ChosenStartMove function - used for opening moves
        }
    }
}
