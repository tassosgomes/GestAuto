import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
// Badge component not installed yet, using placeholder or skipping if not available. Task 2 installed: button, card, input, label, separator, sheet, avatar, dropdown-menu.
// Badge is not in the list of installed components in Task 2. I will skip Badge for now or use a simple div.

export function DesignSystemPage() {
  return (
    <div className="container mx-auto py-10 space-y-10">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight">Design System</h1>
        <p className="text-muted-foreground">
          Documentação visual dos componentes, cores e tipografia do GestAuto.
        </p>
      </div>

      <Separator />

      {/* Cores */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold tracking-tight">Cores</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-primary shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Primary</p>
              <p className="text-xs text-muted-foreground">--primary</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-secondary shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Secondary</p>
              <p className="text-xs text-muted-foreground">--secondary</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-destructive shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Destructive</p>
              <p className="text-xs text-muted-foreground">--destructive</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-muted shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Muted</p>
              <p className="text-xs text-muted-foreground">--muted</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-accent shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Accent</p>
              <p className="text-xs text-muted-foreground">--accent</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-popover border shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Popover</p>
              <p className="text-xs text-muted-foreground">--popover</p>
            </div>
          </div>
          <div className="space-y-2">
            <div className="h-20 w-full rounded-md bg-card border shadow-sm" />
            <div className="space-y-1">
              <p className="font-medium">Card</p>
              <p className="text-xs text-muted-foreground">--card</p>
            </div>
          </div>
        </div>
      </section>

      <Separator />

      {/* Tipografia */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold tracking-tight">Tipografia</h2>
        <div className="space-y-4">
          <div>
            <h1 className="scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl">
              Heading 1
            </h1>
            <p className="text-sm text-muted-foreground">text-4xl font-extrabold tracking-tight lg:text-5xl</p>
          </div>
          <div>
            <h2 className="scroll-m-20 border-b pb-2 text-3xl font-semibold tracking-tight first:mt-0">
              Heading 2
            </h2>
            <p className="text-sm text-muted-foreground">text-3xl font-semibold tracking-tight</p>
          </div>
          <div>
            <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
              Heading 3
            </h3>
            <p className="text-sm text-muted-foreground">text-2xl font-semibold tracking-tight</p>
          </div>
          <div>
            <h4 className="scroll-m-20 text-xl font-semibold tracking-tight">
              Heading 4
            </h4>
            <p className="text-sm text-muted-foreground">text-xl font-semibold tracking-tight</p>
          </div>
          <div>
            <p className="leading-7 [&:not(:first-child)]:mt-6">
              The king, seeing how much happier his subjects were, realized the error of his ways and repealed the joke tax.
            </p>
            <p className="text-sm text-muted-foreground">p leading-7</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">
              Texto de apoio ou descrição (muted-foreground).
            </p>
          </div>
        </div>
      </section>

      <Separator />

      {/* Componentes */}
      <section className="space-y-8">
        <h2 className="text-2xl font-semibold tracking-tight">Componentes</h2>

        {/* Buttons */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium">Buttons</h3>
          <div className="flex flex-wrap gap-4">
            <Button>Default</Button>
            <Button variant="secondary">Secondary</Button>
            <Button variant="destructive">Destructive</Button>
            <Button variant="outline">Outline</Button>
            <Button variant="ghost">Ghost</Button>
            <Button variant="link">Link</Button>
            <Button size="sm">Small</Button>
            <Button size="lg">Large</Button>
            <Button size="icon">Icon</Button>
          </div>
        </div>

        {/* Inputs & Labels */}
        <div className="space-y-4 max-w-sm">
          <h3 className="text-lg font-medium">Inputs</h3>
          <div className="grid w-full max-w-sm items-center gap-1.5">
            <Label htmlFor="email">Email</Label>
            <Input type="email" id="email" placeholder="Email" />
          </div>
          <div className="grid w-full max-w-sm items-center gap-1.5">
            <Label htmlFor="disabled">Disabled</Label>
            <Input disabled type="text" id="disabled" placeholder="Disabled input" />
          </div>
        </div>

        {/* Cards */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium">Cards</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Card Title</CardTitle>
                <CardDescription>Card Description</CardDescription>
              </CardHeader>
              <CardContent>
                <p>Card Content</p>
              </CardContent>
              <CardFooter>
                <p>Card Footer</p>
              </CardFooter>
            </Card>
            <Card>
              <CardHeader>
                <div className="flex items-center gap-4">
                  <Avatar>
                    <AvatarImage src="/avatars/01.png" />
                    <AvatarFallback>CN</AvatarFallback>
                  </Avatar>
                  <div>
                    <CardTitle>User Profile</CardTitle>
                    <CardDescription>With Avatar</CardDescription>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <p>This card demonstrates the usage of Avatar within a Card component.</p>
              </CardContent>
              <CardFooter>
                <Button className="w-full">Action</Button>
              </CardFooter>
            </Card>
          </div>
        </div>
      </section>
    </div>
  );
}
