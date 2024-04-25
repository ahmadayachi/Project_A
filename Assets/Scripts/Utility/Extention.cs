using Fusion;
using System.Linq;
using UnityEngine;

public static class Extention
{
    /// <summary>
    /// returns true if the two types are the same
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="X"></typeparam>
    /// <param name="Type1"></param>
    /// <param name="type2"></param>
    /// <returns></returns>
    public static bool AreSameType<T, X>(T typeToCheck) => typeToCheck is X;

    public static bool TryCastToStruct<T, X>(T structToCast, out X result) where X : struct
    {
        if (structToCast is X)
        {
            result = (X)(object)structToCast;
            return true;
        }
        else
        {
            //default is null
            result = default;
            return false;
        }
    }

    public static bool IsAValidBeloteRank(byte rank)
    {
        if (rank == 1) return true;
        if (rank >= 7 && rank <= 13) return true;
        return false;
    }

    /// <summary>
    /// Cast any byte to Card Suit Enum
    /// </summary>
    /// <param name="suitNumber"></param>
    /// <returns></returns>
    public static CardSuit ByteToCardSuit(byte suitNumber)
    {
        if (suitNumber == 0)
        {
            return CardSuit.NoSuit;
        }
        else
        {
            var Suit = (((suitNumber - 1) % 4) + 1);
            return (CardSuit)Suit;
        }
    }

    #region Cards Array extentions

    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public static void Shuffle<T>(this T[] array)
    {
        System.Random rng = new System.Random();
        int length = array.Length - 1;
        for (int index = length; index > 0; index--)
        {
            int RandomIndex = rng.Next(index + 1);
            T temp = array[index];
            array[index] = array[RandomIndex];
            array[RandomIndex] = temp;
        }
    }

    public static bool AddCard(this CardInfo[] array, CardInfo card)
    {
        if (!card.IsValid || array.Contains(card))
            return false;
        bool CardIsAdded = false;
        for (int index = 0; index < array.Length; index++)
        {
            if (!array[index].IsValid)
            {
                array[index] = card;
                CardIsAdded = true;
                break;
            }
        }
        return CardIsAdded;
    }

    /// <summary>
    /// If Cards Are Same (returns True ) , (Returns False ) if either Cards are not valid
    /// </summary>
    /// <param name="firstCard"></param>
    /// <param name="secondCard"></param>
    /// <returns></returns>
    public static bool AreSameCard(CardInfo firstCard, CardInfo secondCard)
    {
        if (!firstCard.IsValid || !secondCard.IsValid)
            return false;

        return (firstCard.ID == secondCard.ID &&
                firstCard.Rank == secondCard.Rank &&
                firstCard.Suit == secondCard.Suit);
    }

    public static bool RemoveCard(this CardInfo[] array, CardInfo cardToRemove)
    {
        //blocking if card is not instentiated
        if (cardToRemove.IsValid) return false;

        bool IsCardRemoved = false;

        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], cardToRemove))
            {
                array[index] = new CardInfo();
                IsCardRemoved = true;
                break;
            }
        }
        return IsCardRemoved;
    }

    /// <summary>
    /// true if the array is null or size (arrray length is 0)
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsNotInitialized(this CardInfo[] array)
    {
        return (array == null || array.Length == 0);
    }

    /// <summary>
    /// the total Number of Cards that are Valid In this Array
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static int ValidCardsCount(this CardInfo[] array)
    {
        if (array.IsNotInitialized()) return 0;

        int count = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index].IsValid))
                count++;
        }
        return count;
    }

    /// <summary>
    /// true if Valid cards Count is 0
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsEmpty(this CardInfo[] array)
    {
        return array.ValidCardsCount() == 0;
    }

    public static bool ContainsCard(this CardInfo[] array, CardInfo card)
    {
        if (array.IsNotInitialized())
        {
#if Log
            LogManager.LogError("Array is Not Intilialized !");
#endif
            return false;
        }
        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], card))
            { return true; }
        }
        return false;
    }

    public static bool ContainsRank(this CardInfo[] array, byte rank)
    {
        if (array.IsNotInitialized()) return false;
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].Rank == rank)
            { return true; }
        }
        return false;
    }

    public static int DuplicateCounter(this CardInfo[] array, byte rank)
    {
        byte counter = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].Rank == rank)
                counter++;
        }
        return counter;
    }

    #endregion Cards Array extentions

    #region Networked Card Array extentions

    public static int ValidCardsCount(this NetworkArray<CardInfo> array)
    {
        int count = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index].IsValid))
                count++;
        }
        return count;
    }

    public static bool IsEmpty(this NetworkArray<CardInfo> array)
    {
        return array.ValidCardsCount() == 0;
    }

    public static CardInfo[] ToCardInfo(this NetworkArray<CardInfo> array)
    {
        CardInfo[] cards = null;
        if (array.IsEmpty())
        {
            return null;
        }
        else
        {
            cards = new CardInfo[array.ValidCardsCount()];
            for (int index = 0; index < array.Length; index++)
            {
                if (array[index].IsValid)
                {
                    cards[index] = array[index];
                }
            }
        }
        return cards;
    }

    public static void Clear(this NetworkArray<CardInfo> array)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].IsValid)
                array[index] = new CardInfo();
        }
    }

    public static bool AddCard(this NetworkArray<CardInfo> array, CardInfo card)
    {
        if (!card.IsValid)
        {
#if Log
            LogManager.LogError("Attemp to add invalid Card to Array!");
#endif
            return false;
        }
        if (array.Contains(card))
        {
#if Log
            LogManager.LogError("array Already Contains Card!");
#endif
            return false;
        }
        for (int index = 0; index < array.Length; index++)
        {
            if (!array[index].IsValid)
            {
                array[index] = card;
                return true;
            }
        }
        return false;
    }

    public static bool ContainsCard(this NetworkArray<CardInfo> array, CardInfo card)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], card))
                return true;
        }
        return false;
    }

    #endregion Networked Card Array extentions

    #region UI

    /// <summary>
    /// sets a giving child to a giving parent
    /// </summary>
    /// <param name="_transform"></param>
    /// <param name="Parent"></param>
    public static void SetParent(Transform _transform, Transform Parent)
    {
        _transform.SetParent(Parent, false);
    }

    public static void FindCanvasAndSetLastSibling(Transform transform)
    {
        Canvas canvasgo = MonoBehaviour.FindObjectOfType<Canvas>();
        if (canvasgo != null)
        {
            SetParent(transform, canvasgo.transform);
            transform.SetAsLastSibling();
        }
#if Log
        else
            LogManager.LogError("No GameObject with the Name Canvas Have Been Found !");
#endif
    }

    #endregion UI
}