# Release gate correction

The owner has **tested and approved existing preferences behavior**. Do not re-open an unspecified generic defect merely because edge cases were not reproduced in the earlier source review. A new Save preferences button, Restore defaults operation and ignored-app store do introduce new persistence behavior; test those specific new paths, including failure reporting, without conflating them with the approved baseline.

The owner explicitly **excludes version-to-version migration testing** from this release's acceptance. Mark it `OUT OF SCOPE — NOT TESTED`, never `PASS`, and disclose residual upgrade uncertainty if relevant. The developer must still avoid deliberately overwriting existing user data.

Owner will personally test the five-language interface. This is not a waiver for development to verify compilation, localization key/placeholder coverage, app behavior under filtering, storage integrity or package content. No multilingual release claim before the owner completes the actual UI acceptance.

Alt+Tab likeness is a user-visible behavior requiring observation; `EnumWindows` is only a starting point. The older Calculator/Settings exclusion was a workaround for phantom windows, not an intentional product limit. The implementation must resolve both the phantom case and visibility of genuinely open applications.

This change overrides conflicting tests/claims in older `VERIFICATION.md` and `PRODUCT-REVIEW.md` until those historical documents are consolidated. No new tests are marked PASS by this release-gate correction.