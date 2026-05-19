# Gradient Texture Generator

## Setup

Install the package through Unity Package Manager with a Git URL or the local `file:` path used by the test project.

## Usage

Create a generator asset from `Assets/Create/Control Tools/Gradient Texture Generator`. Edit the gradient and width, then reference the generated texture sub-asset wherever a 1D gradient texture is needed.

## Notes

Runtime code contains the generator asset definition. Texture creation and updates compile in the Editor assembly only.
