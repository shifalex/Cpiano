# Design Guide

This project now has two centralized style files:

- `Resources/Styles/GameStyles.xaml`
- `Resources/Styles/KeyboardStyles.xaml`

Think of them as the MAUI equivalent of a small CSS design system.

## Where to start

1. Pick 3-5 base colors first.
2. Update the color tokens in `GameStyles.xaml` and `KeyboardStyles.xaml`.
3. Run the app and look at one number page and one keyboard page.
4. Only after the colors feel right, tweak sizes, radius, and spacing.

## Good first tokens to change

For game screens:

- `GamePageBackgroundColor`
- `GameCardBackgroundColor`
- `GameNumericEntryBackgroundColor`
- `GameNumericEntryActiveBackgroundColor`

For the in-app numeric keypad:

- `NumericKeypadSurfaceColor`
- `NumericKeypadDigitButtonColor`
- `NumericKeypadActionButtonColor`
- `NumericKeypadSubmitButtonColor`

## A simple workflow

1. Choose a mood:
   - calm classroom
   - playful toy
   - clean Android-like utility
2. Set surface colors.
3. Set text contrast.
4. Set one accent color for the main action button.
5. Test on both a narrow phone screen and a wider screen.

## Practical rule

Change only one category at a time:

- colors
- spacing
- corner radius
- typography

That makes it much easier to understand what improved the design.
