# Character Stat System Rework

## Current Issues
- Stats are currently referenced using string names, which is error-prone and lacks type safety
- Multiple dictionaries are used to store and manage stats, leading to scattered stat management
- No centralized stat type system, making it difficult to maintain and extend

## Planned Changes
1. Replace string-based stat references with a proper enum or strongly-typed system
2. Consolidate stat storage into a more cohesive data structure
3. Implement a centralized stat management system
4. Improve type safety throughout the stat system
5. Make stat types more extensible for future additions

## Files to be Modified
- Character stat-related classes
- Stat storage and management systems
- Any systems that reference or modify character stats

## Benefits
- Improved type safety
- Better IDE support (autocomplete, refactoring)
- Reduced chance of runtime errors from typos in stat names
- More maintainable and extensible stat system
- Clearer code organization

## Implementation Notes
- Need to carefully migrate existing stat references
- Ensure backward compatibility with saved character data
- Update tests to reflect the new strongly-typed system
