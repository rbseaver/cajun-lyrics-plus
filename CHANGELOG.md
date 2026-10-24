# Changelog

## [1.4.1](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/compare/v1.4.0...v1.4.1) (2026-10-24)

### Bug Fixes

* **api,web:** never serve a different song than requested ([a2b3ce5](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/a2b3ce5d9486736a98839d794e772eb6149f2ea2))

## [1.4.0](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/compare/v1.3.0...v1.4.0) (2026-10-23)

### Features

* **api:** stamp matched language on lyric results ([9e4996f](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/9e4996f62d4bc739da3aede371aa22337ba0a560))
* **web:** show matched language on lyric card ([a462453](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/a4624534823f422bbfa065767baee2c1ea433056))

## [1.3.0](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/compare/v1.2.0...v1.3.0) (2026-10-22)

### Features

* **api:** search regional French languages by default ([c70b72f](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/c70b72f6bda30f68a8e953772797a248e67b5c9c))
* **web:** move language selection behind advanced search ([eda0ff8](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/eda0ff87d55124ebff2391d4465f43eb3a9dd53d))

## [1.2.0](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/compare/v1.1.0...v1.2.0) (2026-10-11)

### Features

* **api:** add /health endpoint for container probes ([07e1267](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/07e12678ff568cf026c7c6e1014023cb9f01190b))

### Bug Fixes

* **api:** gate https redirection on development only ([8821d8c](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/commit/8821d8c51816df722e0ad339ecd7c67a3ff60d22))

All notable changes to this project will be documented in this file. See [Conventional Commits](https://www.conventionalcommits.org/) for commit guidelines.

## [1.1.0](https://gitlab.com/rbseaver/cajun-lyrics-plus/compare/v1.0.0...v1.1.0) (2026-09-04)


### Features

* **languages:** add support for Acadian French search ([46d5a5a](https://gitlab.com/rbseaver/cajun-lyrics-plus/commit/46d5a5a011a406110f8b1aa0bb192286734336b4))


### Bug Fixes

* correct image in Dockerfile ([b29ad22](https://gitlab.com/rbseaver/cajun-lyrics-plus/commit/b29ad221e8c640e2145f3cc1a7b776c917717b6e))
* fix artist-lyric spacing in search results ([bc2bb28](https://gitlab.com/rbseaver/cajun-lyrics-plus/commit/bc2bb28b8b2c5028b543e19ac2aba47c56e225a1))

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
