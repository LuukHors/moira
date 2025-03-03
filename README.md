# Prototype
This is a prototype.
It only will show how useful this project could be and I believe that in the initial version you always make a lot of mistakes which you can learn from.

# Terms
Header to determine some terms used on this page.
- IDP: Identity Provider.
- IDP object: Any resource you could create in an IDP, examples are: groups, applications, roles, users, scopes, provider, policies, flows, etc. 
- CRD: Custom Resource Definition.
- CR: Custom Resource.

# The problem
When using or creating applications you might need some form of authentication and authorization. To create user registration and the actual logic of validating user login and rbac would cost a lot of time and effort for your company. Assuming you're not Google, Netflix or any big company that could create secure, efficient and up-to-date software to supply your company with these wishes, you might consider using an existing solution. This is where IDPs comes in.

## IDPs
An IDP(Identity Provider) basically let's you have user registration, a login, rbac, OAuth2 flows and many other features out of the box so you do not have to worry about building these solutions yourself. Some of these even will let you use a different service like Google or Microsoft to let them handle the user registration while you can benefit from the login features. 

There are many different types of IDPs. Some examples are:
- Auth0
- Authentik
- KeyCloak
- MS EntraId
- Google

Some of these IDPs are open-source. This makes them relatively easy to self host, at least as the base implementation. Most of them have a image or Helm chart available to be able to host them in a container environment. When hosting such an IDP, you're almost always required supply a database. Because where else will the IDP save these users, groups, applications, etc?

Then to create users, groups and applications, etc, you might have to login, click through a lot of different interfaces, just create a simple user or group, or logical application. Repeat this for every, single, thing, you have and repeat for every application within the company. This, to me, is a lot of effort I don't want to put in.

# The solution
In Kubernetes we can create a CRD that is controlled by an Operator. This Custom Resource (created from the CRD) will then represent the state of the IDP object. This way we can use the Kubernetes' reconciliation loop to determine the desired state and then use a CRUD operation to apply this desired state in the IDP.

With this feature comes the power of GitOps. We can define our IDP object as manifests in Git using a CR. Then use a tool like ArgoCD to create this object in Kubernetes based on the manifests in Git. If we then want to change anything, we can just commit our changes to the git repository let ArgoCD sync it to Kubernetes API. The IDP Operator will then see the change in the desired state of de CR and apply the change in the IDP.

This will be the basic idea of this project.

## Project goals
- Have the state of my IDP in git (use gitops!).
- Be able to automate my IDP object creation using CRD's.

## Personal goals
- Learn more about Kubernetes operators.
- Learn more about IDPs.
- Have a project I can write some code in and 
- Improve the Gitopsability (definitely not a made up word) in Kubernetes.
- Have fun!

# Architecture

## Scope
To begin with, we should define a basic scope for this project.

- Support multiple IDPs.
- For the prototype only support individual groups, applications.
    - Do research on how to use them together.
    - Like having a group of users get permissions on the application itself.
    