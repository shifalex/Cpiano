Run `dotnet run --project Tools/LanguageTests/LanguageTests.csproj` from the app directory.

The harness links the production localization and preference code, with in-memory substitutes for MAUI platform boundaries. It checks per-user isolation, independent text and narration choices, Gripping inheritance and reset, translation coverage for every two-hand instruction, generated magnitude wording, repeated language changes, voice locale selection, and unavailable voices.

Device checks (require the MAUI app and installed system voices):

1. Open user settings and select each text language. Verify labels and Hebrew right-to-left layout; switch back to English.
2. Choose a different narration language and use Preview narration. Install a matching system voice if unavailable.
3. Open Gripping settings. Verify it inherits user choices; select overrides, then restore Use user settings.
4. Open Remember grip changes and its gear settings. Change language without restarting; check labels, selections, sliders, instruction text, and narration on the next prompt.
5. Leave during narration and confirm speech stops. Verify Hebrew does not mirror the keyboard or movement arrows.
6. Switch users and restart the app. Confirm each user's choices are retained separately on this device.

Preferences are device-local, like the existing numeric keyboard preference. They are not synchronized between devices. Translation keys are English source text; game identifiers and mathematical notation remain unchanged.
