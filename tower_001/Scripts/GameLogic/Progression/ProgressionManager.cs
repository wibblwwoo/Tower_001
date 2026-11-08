[Previous content remains the same until the CanPrestige method...]

    public bool CanPrestige(string characterId, int prestigeLevel)
    {
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return false;

        return progress.CheckPrestigeRequirements(prestigeLevel);
    }

    private float GetExperienceMultiplier(string characterId, ExperienceSource source)
    {
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return 1.0f;

        return progress.CalculateExperienceMultiplier(source);
    }

[Rest of the content remains the same...]
