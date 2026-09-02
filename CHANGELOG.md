# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [1.0.0](https://gitlab.com/rbseaver/cajun-lyrics-plus/compare/v0.0.0...v1.0.0) (2026-09-02)


### Features

* add search endpoint and multi-result selection workflow to API and UI ([aeab1f9](https://gitlab.com/rbseaver/cajun-lyrics-plus/commit/aeab1f96dd964365ee1e79ea9df5a8e47e033df5))

## 0.0.0 (2026-09-01)

### Features

* add ability to search by language
* add api and controller to return lyrics
* add extension method to check for null or empty before adding to a collection
* add Swagger/Scalar support for api docs
* **api:** add CORS policy for React dev server
* **api:** redirect root path to Scalar docs and enable launchUrl
* **config:** add HttpClientOptions and centralize base URL
* **controller:** implement basic error handling
* make request fields nullable, update querystring builder, simplify response mock in test
* **models:** add LyricLanguage smart enum for language filtering
* package app in docker container
* refactor API client and improve test coverage
* **web:** scaffold React frontend with Vite and TypeScript

### Bug Fixes

* read response as byte array to workaround server misconfiguration
* register dependencies, remove unused boilerplate controller
* **test:** add includeLyric param to BuildLyricResults
