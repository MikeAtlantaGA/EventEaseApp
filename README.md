
# How GitHub Copilot Helped Build EventEaseApp

GitHub Copilot accelerated development by:

1. Generating initial data models and reusable UI components.
2. Automating CRUD logic for events and attendee registrations.
3. Building advanced UI features (sortable/searchable tables, modals, attendee management).
4. Integrating browser storage for persistent state.
5. Enhancing UI/UX with icons, tooltips, and modern styling.
6. Guiding robust validation, error handling, and resolving build/runtime issues.
7. Suggesting performance optimizations and best practices.
8. Assisting with unit test setup and GitHub Actions for CI.
9. Providing documentation, improvement suggestions, and guiding major refactors.

Copilot served as a coding assistant, reviewer, and problem solver—enabling rapid, high-quality development and learning.

---

# Portfolio Case Study: EventEaseApp

**Project Overview:**
EventEaseApp is a Blazor web application for managing events and attendee registrations. The project was built iteratively with Copilot, focusing on clean architecture, robust features, and a modern user experience.

## Main Steps Taken

1. **Project Setup & Data Modeling**
	- Created the initial Event data model and foundational UI components.
	- Set up navigation and page routing for Events, Attendee Registration, and Session Tracker.

2. **Feature Development**
	- Implemented event CRUD operations and attendee registration workflows.
	- Added sortable/searchable event tables, modal forms, and attendee management features.
	- Integrated browser storage for events, registrations, and session state.

3. **UI/UX Enhancements**
	- Added Bootstrap icons, tooltips, and custom CSS for a polished interface.
	- Used modal dialogs for forms and confirmations to streamline user interactions.

4. **Validation, Error Handling & Testing**
	- Developed robust form validation, including custom date validation logic.
	- Diagnosed and resolved build/runtime errors with Copilot’s guidance.
	- Added lightweight unit tests for core services and migration logic.

5. **Refactoring & Best Practices**
	- Refactored large components into smaller, reusable pieces.
	- Adopted stable identifiers for data relationships and centralized route management.
	- Hardened browser storage handling and improved code organization.

6. **DevOps & Documentation**
	- Configured GitHub Actions for automated test runs.
	- Documented Copilot’s contributions and project structure for future maintainers.

## Key Copilot Prompts Used

> Good day! I'm building a new app called EventEaseApp which will assist the company EventEase to manage event details like event name, date, and location. The app should seamlessly navigate between pages like a master list of events, event details when a particular event is selected, and to register participants for the event. Please start by creating the appropriate data model and an event card that will act as the foundation for the application.

> Please include a button on the Events page to allow the user to enter a new Event. Also provide the ability for the user to edit the details for an existing event or delete it altogether.

> On the Events page, please display the existing events in table format, rather than the card format. Keep the card format for adding or editing events. Please include the ability to sort the event list by any of the three fields and provide search capability for the user to find specific event(s).

> Please implement handlers for the sorting functionality on the Event List page. Also please implement handlers for the search functionality.

> Please add a "Calendar" icon to the navigation menu for the "Events" feature

> Can you update the date validation functionality to only validate the date after the user enters all three parts (month, date, and year) rather than validating each date part?

> Please make the add/edit form a modal pop up rather than inline on the event list page

> Can you add a button to clear search criteria? I'd like it to be in-line and to the right of the search fields in the "Actions" column.

> Please add tool tips to the buttons. Also, please move the user registration button to the registered user's list. For existing registrations, include a delete button to the user registration form itself, hide the delete button if this is a new registration. And finally, please add a confirm/cancel modal pop-up to verify registration deletions.

> Please ensure this code follows current best practices. Please provide suggestions for improvement before implementing them so I can learn for future projects.

> Yes, please proceed with adding some lightweight unit tests on the service layer and migration logic. It would also be great to replace the string literals with centralized route constraints.

> Can you assist me in creating a GitHub repository for this application?

---

**Summary:**
EventEaseApp demonstrates how Copilot can accelerate full-stack web development, from initial scaffolding to advanced features, testing, and deployment. Copilot’s suggestions, code generation, and problem-solving support enabled rapid iteration and high-quality results.
# EventEaseApp

[![dotnet test](https://github.com/MikeAtlantaGA/EventEaseApp/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/MikeAtlantaGA/EventEaseApp/actions/workflows/dotnet-test.yml)

EventEaseApp is a Blazor WebAssembly event-management application built as a course project and expanded into a more complete portfolio piece focused on event operations, attendee registration, client-side persistence, and maintainable front-end architecture.

## Portfolio Case Study

### Overview

This project started as a basic Blazor front-end assignment and evolved into a polished event-management application. The goal was to move beyond template-level CRUD and build something that felt closer to a real product: event creation and search, attendee registration flows, browser-persisted state, cleaner navigation, reusable modal components, and lightweight automated testing.

### Problem

The original app structure was functional but limited. It behaved more like a starter project than a focused application. Several issues became clear during iteration:

- form validation was too eager and interrupted users while typing
- event and attendee workflows were too disconnected
- sample-template navigation and pages diluted the product focus
- state was not persisted in a way that made the app feel realistic
- growing UI complexity was pushing too much logic into a single page

### Goals

- create a more intentional event-management experience
- support attendee registration directly within event workflows
- persist events, registrations, and session state in browser storage
- improve usability with better search, clearer actions, and confirmation flows
- refactor the app toward cleaner components and safer service-layer logic
- add baseline automated tests and CI validation

### My Role

I drove the product direction, UX refinement, architecture cleanup, and delivery flow for the application. That included translating iterative feature ideas into implementation steps, reviewing technical tradeoffs, refactoring weak spots, and adding testing and GitHub delivery infrastructure.

### Key Features

- event creation, editing, deletion, and search
- attendee registration per event with edit and delete flows
- attendee count visibility from the event list
- browser-persisted event data, registration data, and session state
- reusable modals for event forms, attendee lists, and confirmations
- reset tooling for stored sample/demo data
- simplified navigation and stronger EventEase branding

### Technical Decisions

#### 1. Improve UX before adding more features

One of the earliest decisions was to stop validating event dates while the user was still typing. That led to a simpler save-time validation approach and a more predictable form experience.

#### 2. Move registration into the event workflow

Instead of treating attendee registration as a separate page, registration was moved into the context of each event. This reduced navigation friction and made the app feel centered on the event lifecycle rather than on disconnected forms.

#### 3. Persist meaningful state in browser storage

The app was extended to persist:

- events
- attendee registrations
- search/session state

This made the project behave more like a real single-page application and less like a classroom demo.

#### 4. Refactor relationships to use stable identifiers

Attendee registrations were originally tied to event names. That was fragile, especially when names changed. The model was refactored to use `EventId` as the relationship key, and migration logic was added to preserve older browser-stored data.

#### 5. Split large page logic into cleaner components

As the Events screen grew, its responsibilities were separated into reusable components and a code-behind partial class. That reduced coupling and made the UI easier to extend and test.

### Architecture Highlights

- `Pages/Events.razor` and `Pages/Events.razor.cs` split UI orchestration from markup
- reusable modal components handle event forms, attendee lists, and confirmations
- service layer isolates event, registration, session, storage, and tooltip behavior
- `AppRoutes` centralizes route constants used outside Razor page directives
- safer storage deserialization prevents corrupt browser data from breaking the app

### Testing And Delivery

To make the project more portfolio-ready, I added lightweight service-layer tests and repository automation:

- xUnit test project for core service behavior
- tests for event persistence, registration migration, and corrupt storage recovery
- GitHub Actions workflow to run `dotnet test` on pushes and pull requests
- published GitHub repository with CI badge in this README

### Challenges Solved

- reduced noisy validation behavior in forms
- avoided brittle event-to-attendee relationships by migrating to `EventId`
- cleaned up a large, hard-to-maintain page by extracting components
- preserved existing stored data through migration logic instead of breaking changes
- turned a local course project into a shareable, tested GitHub portfolio artifact

### Outcome

EventEaseApp became more than a coursework exercise. It now demonstrates product thinking, UI refinement, client-side state management, refactoring discipline, migration-aware data modeling, and basic CI-backed testing in a Blazor WebAssembly application.

### Tech Stack

- Blazor WebAssembly
- C# / .NET 10
- Razor components
- Bootstrap and Bootstrap Icons
- xUnit
- GitHub Actions

### Repository

- GitHub: [MikeAtlantaGA/EventEaseApp](https://github.com/MikeAtlantaGA/EventEaseApp)
