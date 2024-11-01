﻿namespace Server;

public static class NameCreator
{
    private static readonly List<string> adjectives = ["Angry", "Funny", "Big", "Tiny", "Spectacular", "Green", "Spotted", "Magnificent", "Long", "Sarcastic", "Nasty", "Proud", "Friendly"];
    public static readonly List<string> animals = ["Hippo", "Anaconda", "Horse", "Gorilla", "Blobfish", "Snail", "Hedgehog", "Squirrel", "Velociraptor"];

    public static string CreateName()
    {
        return $"{adjectives.PickRandom()} {animals.PickRandom()}";
    }
}

public static class EnumerableExtension
{
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }
}