using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== DAILY NUMBER ORACLE: BRUTAL PROTOTYPE ===");
        Console.WriteLine("5 guesses. Clues must be earned by closeness.\n");

        var levels = new List<Level>
        {
            new Level(
                masterNumber: 42,
                category: "Pop Culture",
                cluesByTier: new List<string>
                {
                    "A cultural pebble that caused an outsized ripple.",
                    "It is even, and unusually famous for being an answer.",
                    "It is not a round number, but it’s oddly satisfying.",
                    "It equals the product of two small, familiar integers.",
                    "A certain guidebook would call this… definitive."
                },
                revealText: "42 — In *The Hitchhiker’s Guide to the Galaxy*, it’s the Answer to the Ultimate Question of Life, the Universe, and Everything."
            ),

            new Level(
                masterNumber: 1969,
                category: "Science / Space",
                cluesByTier: new List<string>
                {
                    "A year that made the sky feel closer.",
                    "20th century. Not late-century.",
                    "It involves a journey where cameras mattered.",
                    "A mission number becomes legendary here.",
                    "A giant leap happened: Apollo 11, Moon landing."
                },
                revealText: "1969 — The year humans first landed on the Moon (Apollo 11)."
            ),

            new Level(
                masterNumber: 1066,
                category: "History",
                cluesByTier: new List<string>
                {
                    "A date that rewired a nation’s story.",
                    "A four-digit year, medieval-adjacent.",
                    "This year is taught as a ‘you must know this’ milestone.",
                    "A decisive battle is the usual shorthand.",
                    "1066 — The Norman Conquest; Battle of Hastings."
                },
                revealText: "1066 — The Norman Conquest of England (Battle of Hastings)."
            ),

            new Level(
                masterNumber: 299_792_458,
                category: "Physics",
                cluesByTier: new List<string>
                {
                    "A number that behaves like a ceiling.",
                    "It’s defined exactly, not measured approximately.",
                    "It’s tied to how we define a unit of length.",
                    "It’s a speed limit with a very famous letter nearby.",
                    "299,792,458 — speed of light in vacuum (m/s)."
                },
                revealText: "299,792,458 — The speed of light in vacuum, in meters per second."
            ),
        };

        foreach (var level in levels)
        {
            PlayLevel(level);
            Console.WriteLine("\n---------------------------------\n");
        }

        Console.WriteLine("All levels complete. Press any key to exit...");
        Console.ReadKey();
    }

    static void PlayLevel(Level level)
    {
        const int MaxGuesses = 5;

        Console.WriteLine($"Category: {level.Category}");
        Console.WriteLine("Rules:");
        Console.WriteLine("- 5 guesses.");
        Console.WriteLine("- You always get feedback: Higher/Lower.");
        Console.WriteLine("- Clues are NOT automatic: you must earn stronger clues by being close.");
        Console.WriteLine("- Repeating a guess burns a turn.\n");

        var seenGuesses = new HashSet<long>();

        // Tier starts at 0 (vaguest). You can unlock higher tiers by closeness.
        int unlockedTier = 0;
        bool won = false;

        for (int i = 1; i <= MaxGuesses; i++)
        {
            Console.Write($"Guess {i}/{MaxGuesses}: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (!long.TryParse(input, out long guess))
            {
                Console.WriteLine("Enter a valid whole number. (No turn lost.)\n");
                i--;
                continue;
            }

            // Brutal but fair: repeated guess consumes the attempt.
            if (!seenGuesses.Add(guess))
            {
                Console.WriteLine("You already spoke that number. The Oracle does not repeat itself.\n");
                continue;
            }

            if (guess == level.MasterNumber)
            {
                Console.WriteLine("\n🏆 Correct!");
                Console.WriteLine(level.RevealText);
                won = true;
                break;
            }

            // Higher/Lower feedback (keeps it solvable & “daily-game” friendly)
            Console.WriteLine(guess < level.MasterNumber ? "Feedback: Higher." : "Feedback: Lower.");

            // Earn clue tier based on *relative closeness*.
            // For small numbers, absolute closeness matters too, so we blend in an absolute bonus.
            double rel = RelativeCloseness(guess, level.MasterNumber); // 0..1 (bigger is closer)

            // Tuning knobs for brutality:
            // - You only unlock meaningful clue upgrades when you're fairly close.
            // - If you're far, you might get "static" and no upgrade.
            int tierGain = TierGainFromCloseness(rel, Math.Abs(guess - level.MasterNumber));

            if (tierGain <= 0)
            {
                Console.WriteLine("Clue: Static. You’re not in the right neighborhood.\n");
            }
            else
            {
                unlockedTier = Math.Min(level.CluesByTier.Count - 1, unlockedTier + tierGain);
                Console.WriteLine($"Clue: {level.CluesByTier[unlockedTier]}\n");
            }
        }

        if (!won)
        {
            Console.WriteLine("The Oracle closes its eye.");
            Console.WriteLine("Truth (revealed at the end for testing):");
            Console.WriteLine(level.RevealText);
        }
    }

    // Relative closeness: 1.0 means exact, approaches 0 as you get far.
    static double RelativeCloseness(long guess, long answer)
    {
        // Use max(|answer|,1) to avoid divide-by-zero and keep stable for small answers.
        double denom = Math.Max(Math.Abs((double)answer), 1.0);
        double diff = Math.Abs((double)guess - answer);
        double ratio = diff / denom;

        // Convert to closeness score: 0..1
        // If you're 0% off => 1.0, if you're 100% off => 0.0
        double closeness = 1.0 - ratio;
        if (closeness < 0) closeness = 0;
        if (closeness > 1) closeness = 1;
        return closeness;
    }

    static int TierGainFromCloseness(double relCloseness, long absDiff)
    {
        // “A hint more brutal”:
        // - Far guesses: no clue upgrade (static)
        // - Medium guesses: +1 tier
        // - Very close: +2 or +3 tiers

        // Absolute mercy for small-number answers:
        // if you're within 3, you get at least +1 tier (unless you're repeating, which is handled elsewhere).
        bool absoluteMercy = absDiff <= 3;

        // Relative thresholds:
        // relCloseness ~ 0.90 means within 10% of answer
        if (relCloseness >= 0.98) return 3;        // within ~2%
        if (relCloseness >= 0.94) return 2;        // within ~6%
        if (relCloseness >= 0.88) return 1;        // within ~12%

        if (absoluteMercy) return 1;

        return 0; // static
    }
}

class Level
{
    public long MasterNumber;
    public string Category;
    public List<string> CluesByTier;
    public string RevealText;

    public Level(long masterNumber, string category, List<string> cluesByTier, string revealText)
    {
        MasterNumber = masterNumber;
        Category = category;
        CluesByTier = cluesByTier;
        RevealText = revealText;
    }
}