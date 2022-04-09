# Contributing to Anamnesis

Thank you for taking the time to contribute to the project! We welcome any reasonable contribution to the repository, and we have a few guidelines in place.

## Table of Contents
- [Can I have a TL;DR?](#can-i-have-a-tldr-i-only-want-to-ask-something)
- [How can I contribute to Anamnesis?](#how-can-i-contribute-to-anamnesis)
  - [Reporting bugs](#reporting-bugs)
    - [Before submitting a bug report](#before-submitting-a-bug-report)
    - [Submitting a bug report](#submitting-a-bug-report)
  - [Suggesting enhancements to existing features, within reason](#suggesting-enhancements-to-existing-features-within-reason)
    - [Before making a feature request](#before-making-a-feature-request)
    - [Submitting a feature request](#submitting-a-feature-request)
  - [Code contributions and pull requests](#code-contributions-and-pull-requests)
    - [Getting Anamnesis set up on your machine](#getting-anamnesis-set-up-on-your-machine)
    - [Submitting a pull request](#submitting-a-pull-request)


## Can I have a TL;DR? I only want to ask something.
Our [Wiki](https://github.com/imchillin/Anamnesis/wiki) is our primary knowledge base, and should always be checked before approaching the team for assistance. This wiki is updated frequently.

> **Please do not open issues/tickets to ask questions. If you have a question that cannot be answered by reading through the documentation provided, [please contact a developer directly on Discord](https://discord.gg/KvGJCCnG8t).** 

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sup>↑ _[back to top](#table-of-contents)</sup>_

## How can I contribute to Anamnesis?
Anamnesis is currently in open beta, so there are various ways you can contribute to the project:

### Reporting bugs
Bugs are an inevitability of any project, but proper reporting and rigorous testing help to resolve bugs quickly and efficiently. However, not every issue you encounter is a bug, so it's best practice to make sure that your reports are as informative and accurate as possible, and ensure that you've eliminated user error as a cause.

#### Before submitting a bug report
- Check the [wiki](https://github.com/imchillin/Anamnesis/wiki), especially the [Troubleshooting](https://github.com/imchillin/Anamnesis/wiki/Troubleshooting) and [FAQ](https://github.com/imchillin/Anamnesis/wiki/FAQ) pages. Many issues are simply a matter of not fully understanding the tool or how it works, and can be easily resolved with a bit of light reading.
- Check the [issue list](https://github.com/imchillin/Anamnesis/issues?q=is%3Aissu). A bug you've experienced may have already been reported, and duplicate issues tend to impede development. If the issue already exists, add to the existing issue to keep things tidy. Make sure to check closed issues as well, as your issue may have been resolved but has not been released.

#### Submitting a bug report
- Click on the Issues tab, then click on the New Issue button
- Click on `Get Started` in the `Bug report` section
- Add a clear and descriptive title for the issue to identify the problem
- Follow the template and complete your bug report, ensuring you have included as much detail as possible- it is better to add too much information than not enough
  - Describe the bug clearly, and advise how many times you encountered the bug- let us know what you were doing when it happened
  - List the exact steps you took to produce the bug- if possible, try to replicate it yourself to make sure you have fully documented these steps
  - Tell us what you expected to happen when you attempted to carry out your original action
- [Attach relevant log files](https://github.com/imchillin/Anamnesis/wiki/Troubleshooting#accessing-log-files-for-issue-reporting) by dragging and dropping them into the text box- these log files include vital information that the developers need to address issues when reported
- Click on `Submit new issue`
- A developer will review the report and label it appropriately as needed where it will then be resolved at the earliest opportunity, or closed in the event of a duplicate ticket or user error report

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sup>↑ _[back to top](#table-of-contents)</sup>_

### Suggesting enhancements to existing features, within reason

#### Before making a feature request
Please take into consideration that there are features that the developers cannot/will not add into the tool for various reasons including but not limited to ethical concerns regarding unfair gameplay advantages and game engine limitations. You are encouraged to [read this list of features](https://github.com/imchillin/Anamnesis/wiki/Before-Making-a-Suggestion), as well the [known list of limitations](https://github.com/imchillin/Anamnesis/wiki/What-is-Anamnesis%3F#limitations). If a feature request cannot be feasibly taken on board because of these issues and/or limitations, you will be informed in the request, but it will be closed.

Beyond this, please also take into consideration that the developers work on Anamnesis in their free time, with no financial incentive, so feature requests will be placed into a priority list, or may not be added at all. However, if you are moderately proficient in C# programming, you may want to consider contributing code to add these features yourself.

> **Checking the Issues list is also imperative, as features are listed here with the `Enhancement` label. If someone has already made the same or a similar request, please add your support to the existing request.**

#### Submitting a feature request
- Click on the Issues tab, then click on the New Issue button
- Click on `Get Started` in the `Feature request` section
- Add a clear and descriptive title for the request
- Follow the template and complete your feature request, ensuring you have included as much detail as possible
  - If your feature request is intended to address a fundamental issue with the usability of the tool, please explain how this issue affects you so we can gain a better understanding of how to best implement the request
  - Describe how you would like the feature to work- if you've done a mock-up, you are more than welcome to drag and drop images showing this into the request form as well
- Click on `Submit new issue`
- A developer will review the request and add additional labels as needed, or close the request if it's been requested before  or cannot/will not be implemented due to limitations or ethical issues.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sup>↑ _[back to top](#table-of-contents)</sup>_

### Code contributions and pull requests
You may wish to contribute to the project in a more developmental capacity. Please feel free to [discuss any potential updates](https://discord.gg/KvGJCCnG8t) you wish to make to the tool prior to submitting a pull request with the developers.

If you are simply using Anamnesis for its intended purpose and have no plans to develop for the project, this section is unlikely to be of use to you.

#### Getting Anamnesis set up on your machine
The following is a list of requirements for getting Anamnesis set up on your machine for development:
- Knowledge of C#
- Visual Studio 2019 or newer with .NET Desktop Development workload

It is recommended that you use [Github Desktop](https://desktop.github.com/) to facilitate smooth pull requests.

Once you have forked the repo and pulled the source code:
1. Run `UpdateSubmodules.bat`
2. Open `Anamnesis.sln` as a solution in Visual Studio 

#### Submitting a pull request
A template has been provided for submitting pull requests. It would be greatly appreciated if you complete this template.

The instructions for creating a pull request have been [documented fully by Github](https://docs.github.com/en/github/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request#creating-the-pull-request), so it is recommended that you read their instructions.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<sup>↑ _[back to top](#table-of-contents)</sup>_
